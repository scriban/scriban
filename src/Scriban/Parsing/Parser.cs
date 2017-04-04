// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Scriban.Runtime;

namespace Scriban.Parsing
{
    /// <summary>
    /// The parser.
    /// </summary>
    public partial class Parser
    {
        private readonly Lexer lexer;
        private Lexer.Enumerator tokenIt;
        private readonly LinkedList<Token> tokensPreview;
        private Token previousToken;
        private Token token;
        private bool inCodeSection = false;
        private int blockLevel;
        private bool inFrontMatter = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public Parser(Lexer lexer, ParserOptions options = null)
        {
            if (lexer == null) throw new ArgumentNullException(nameof(lexer));
            this.lexer = lexer;
            tokensPreview = new LinkedList<Token>();
            Messages = new List<LogMessage>();

            Options = options ?? new ParserOptions();
            CurrentParsingMode = lexer.Options.Mode;

            Blocks = new Stack<ScriptNode>();

            // Initialize the iterator
            tokenIt = lexer.GetEnumerator();
            NextToken();
        }

        public ParserOptions Options { get; }

        public List<LogMessage> Messages { get; private set; }

        public bool HasErrors { get; private set; }

        private Stack<ScriptNode> Blocks { get; }

        private Token Current => token;

        private Token Previous => previousToken;

        public SourceSpan CurrentSpan => GetSpanForToken(Current);

        private ScriptMode CurrentParsingMode { get; set; }

        public ScriptPage Run()
        {
            Messages = new List<LogMessage>();
            HasErrors = false;
            blockLevel = 0;
            Blocks.Clear();

            var page = Open<ScriptPage>();
            var parsingMode = CurrentParsingMode;
            switch (parsingMode)
            {
                case ScriptMode.FrontMatterAndContent:
                case ScriptMode.FrontMatterOnly:
                    if (Current.Type != TokenType.FrontMatterMarker)
                    {
                        LogError($"When [{CurrentParsingMode}] is enabled, expecting a `{lexer.Options.FrontMatterMarker}` at the beginning of the text instead of `{Current.GetText(lexer.Text)}`");
                        return null;
                    }

                    inFrontMatter = true;
                    inCodeSection = true;

                    // Skip the frontmatter marker
                    NextToken();

                    // Parse the front matter
                    page.FrontMatter = ParseBlockStatement(null);

                    // We should not be in a frontmatter after parsing the statements
                    if (inFrontMatter)
                    {
                        LogError($"End of frontmatter `{lexer.Options.FrontMatterMarker}` not found");
                    }

                    if (parsingMode == ScriptMode.FrontMatterOnly)
                    {
                        return page;
                    }
                    break;
                case ScriptMode.ScriptOnly:
                    inCodeSection = true;
                    break;
            }

            ParseBlockStatement(page);

            if (page.FrontMatter != null)
            {
                FixRawStatementAfterFrontMatter(page);
            }

            if (lexer.HasErrors)
            {
                foreach (var lexerError in lexer.Errors)
                {
                    Log(lexerError);
                }
            }

            return !HasErrors ? page : null;
        }

        private void FixRawStatementAfterFrontMatter(ScriptPage page)
        {
            // In case of parsing a front matter, we don't want to include any \r\n after the end of the front-matter
            // So we manipulate back the syntax tree for the expected raw statement (if any), otherwise we can early 
            // exit.
            var rawStatement = page.Statements.FirstOrDefault() as ScriptRawStatement;
            if (rawStatement == null)
            {
                return;
            }

            var startOffset = rawStatement.Span.Start.Offset;
            var endOffset = rawStatement.Span.End.Offset;
            for (int i = startOffset; i <= endOffset; i++)
            {
                var c = rawStatement.Text[i];
                if (c == ' ' || c == '\t')
                {
                    continue;
                }
                if (c == '\r')
                {
                    if (i + 1 <= endOffset && rawStatement.Text[i+1] == '\n')
                    {
                        rawStatement.Span.Start = new TextPosition(i + 2, rawStatement.Span.Start.Line + 1, 0);
                    }
                    break;
                }

                if (c == '\n')
                {
                    rawStatement.Span.Start = new TextPosition(i + 1, rawStatement.Span.Start.Line + 1, 0);
                }
                break;
            }
        }

        private bool TryParseStatement(ScriptStatement parent, out ScriptStatement statement, out bool hasEnd)
        {
            hasEnd = false;
            bool nextStatement = true;
            statement = null;
            if (HasErrors)
            {
                return false;
            }

            continueParsing:
            switch (Current.Type)
            {
                case TokenType.Eof:
                    // Early exit
                    nextStatement = false;
                    break;

                case TokenType.Raw:
                    statement = ParseRawStatement();
                    break;

                case TokenType.CodeEnter:
                    if (inCodeSection)
                    {
                        LogError("Unexpected token while already in a code block");
                    }
                    inCodeSection = true;
                    NextToken();
                    goto continueParsing;

                case TokenType.FrontMatterMarker:
                    if (inFrontMatter)
                    {
                        inFrontMatter = false;
                        inCodeSection = false;
                        // When we expect to parse only the front matter, don't try to tokenize the following text
                        // Keep the current token as the code exit of the front matter
                        if (CurrentParsingMode != ScriptMode.FrontMatterOnly)
                        {
                            NextToken();
                        }

                        if (CurrentParsingMode == ScriptMode.FrontMatterAndContent || CurrentParsingMode == ScriptMode.FrontMatterOnly)
                        {
                            // Once the FrontMatter has been parsed, we can switch to default parsing mode.

                            CurrentParsingMode = ScriptMode.Default;
                            nextStatement = false;
                        }
                    }
                    else
                    {
                        LogError($"Unexpected frontmatter marker `{lexer.Options.FrontMatterMarker}` while not inside a frontmatter");
                        NextToken();
                    }
                    break;

                case TokenType.CodeExit:
                    if (!inCodeSection)
                    {
                        LogError("Unexpected code block exit '}}' while no code block enter '{{' has been found");
                    }
                    else if (CurrentParsingMode == ScriptMode.ScriptOnly)
                    {
                        LogError("Unexpected code clock exit '}}' while parsing in script only mode. '}}' is not allowed.");
                    }

                    inCodeSection = false;
                    NextToken();
                    goto continueParsing;

                default:
                    if (inCodeSection)
                    {
                        switch (Current.Type)
                        {
                            case TokenType.NewLine:
                            case TokenType.SemiColon:
                                NextToken();
                                goto continueParsing;
                            case TokenType.Identifier:
                            case TokenType.IdentifierSpecial:
                                var identifier = GetAsText(Current);
                                switch (identifier)
                                {
                                    case "end":
                                        hasEnd = true;
                                        nextStatement = false;
                                        NextToken();
                                        break;
                                    case "wrap":
                                        statement = ParseWrapStatement();
                                        break;
                                    case "if":
                                        statement = ParseIfStatement();
                                        break;
                                    case "else":
                                        var parentCondition = parent as ScriptConditionStatement;
                                        if (parentCondition == null)
                                        {
                                            nextStatement = false;

                                            // unit test: 201-if-else-error3.txt
                                            LogError("A else condition must be preceded by another if/else condition");
                                        }
                                        else
                                        {
                                            var nextCondition = ParseElseStatement();
                                            parentCondition.Else = nextCondition;
                                            hasEnd = true;
                                        }
                                        break;
                                    case "for":
                                        if (PeekToken().Type == TokenType.Dot)
                                        {
                                            statement = ParseExpressionStatement();
                                        }
                                        else
                                        {
                                            statement = ParseForStatement();
                                        }
                                        break;
                                    case "with":
                                        statement = ParseWithStatement();
                                        break;
                                    case "import":
                                        statement = ParseImportStatement();
                                        break;
                                    case "readonly":
                                        statement = ParseReadOnlyStatement();
                                        break;
                                    case "while":
                                        if (PeekToken().Type == TokenType.Dot)
                                        {
                                            statement = ParseExpressionStatement();
                                        }
                                        else
                                        {
                                            statement = ParseWhileStatement();
                                        }
                                        break;
                                    case "break":
                                        statement = Open<ScriptBreakStatement>();
                                        NextToken();
                                        Close(statement);

                                        // This has to be done at execution time, because of the wrap statement
                                        //if (!IsInLoop())
                                        //{
                                        //    LogError(statement, "Unexpected statement outside of a loop");
                                        //}
                                        ExpectEndOfStatement(statement);
                                        break;
                                    case "continue":
                                        statement = Open<ScriptContinueStatement>();
                                        NextToken();
                                        Close(statement);

                                        // This has to be done at execution time, because of the wrap statement
                                        //if (!IsInLoop())
                                        //{
                                        //    LogError(statement, "Unexpected statement outside of a loop");
                                        //}
                                        ExpectEndOfStatement(statement);
                                        break;
                                    case "func":
                                        statement = ParseFunctionStatement(false);
                                        break;
                                    case "ret":
                                        statement = ParseReturnStatement();
                                        break;
                                    case "capture":
                                        statement = ParseCaptureStatement();
                                        break;
                                    default:
                                        // Otherwise it is an expression statement
                                        statement = ParseExpressionStatement();
                                        break;
                                }
                                break;
                            default:
                                if (StartAsExpression())
                                {
                                    statement = ParseExpressionStatement();
                                }
                                else
                                {
                                    nextStatement = false;
                                    LogError($"Unexpected token {Current.Type}");
                                }
                                break;
                        }
                    }
                    else
                    {
                        nextStatement = false;
                        LogError($"Unexpected token {Current.Type} while not in a code block {{ ... }}");
                        // LOG an ERROR. Don't expect any other tokens outside a code section
                    }
                    break;
            }

            return nextStatement;
        }

        private ScriptReadOnlyStatement ParseReadOnlyStatement()
        {
            var readOnlyStatement = Open<ScriptReadOnlyStatement>();
            NextToken(); // Skip readonly keyword

            readOnlyStatement.Variable = ExpectAndParseVariable(readOnlyStatement);
            ExpectEndOfStatement(readOnlyStatement);

            return Close(readOnlyStatement);
        }

        private ScriptCaptureStatement ParseCaptureStatement()
        {
            var captureStatement = Open<ScriptCaptureStatement>();
            NextToken(); // Skip capture keyword

            // unit test: 231-capture-error1.txt
            captureStatement.Target = ExpectAndParseExpression(captureStatement);
            ExpectEndOfStatement(captureStatement);
            captureStatement.Body = ParseBlockStatement(captureStatement);

            return Close(captureStatement);
        }

        private ScriptImportStatement ParseImportStatement()
        {
            var importStatement = Open<ScriptImportStatement>();
            NextToken(); // skip import

            importStatement.Expression = ExpectAndParseExpression(importStatement);
            ExpectEndOfStatement(importStatement);

            return Close(importStatement);
        }

        private ScriptWithStatement ParseWithStatement()
        {
            var withStatement = Open<ScriptWithStatement>();
            NextToken();
            withStatement.Name = ExpectAndParseExpression(withStatement);

            if (ExpectEndOfStatement(withStatement))
            {
                withStatement.Body = ParseBlockStatement(withStatement);
            }
            return Close(withStatement);
        }

        private ScriptVariable ExpectAndParseVariable(ScriptNode parentNode)
        {
            if (parentNode == null) throw new ArgumentNullException(nameof(parentNode));
            if (Current.Type == TokenType.Identifier || Current.Type == TokenType.IdentifierSpecial)
            {
                var variableOrLiteral = ParseVariableOrLiteral();
                var variable = variableOrLiteral as ScriptVariable;
                if (variable != null && variable.Scope != ScriptVariableScope.Loop)
                {
                    return (ScriptVariable)variableOrLiteral;
                }
                LogError(parentNode, $"Unexpected variable [{variableOrLiteral}]");
            }
            else
            {
                LogError(parentNode, $"Expecting a variable instead of [{Current.Type}]");
            }
            return null;
        }

        private bool ExpectEndOfStatement(ScriptNode statement)
        {
            if (Current.Type == TokenType.NewLine || Current.Type == TokenType.CodeExit || Current.Type == TokenType.SemiColon || Current.Type == TokenType.Eof)
            {
                return true;
            }
            LogError(statement, $"Expecting <EOL>/end of line after");
            return false;
        }

        private ScriptFunction ParseFunctionStatement(bool isAnonymous)
        {
            var scriptFunction = Open<ScriptFunction>();
            NextToken(); // skip func or do

            if (!isAnonymous)
            {
                scriptFunction.Name = ExpectAndParseVariable(scriptFunction);
            }
            ExpectEndOfStatement(scriptFunction);

            scriptFunction.Body = ParseBlockStatement(scriptFunction);
            return Close(scriptFunction);
        }

        private ScriptExpressionStatement ParseExpressionStatement()
        {
            var expressionStatement = Open<ScriptExpressionStatement>();
            expressionStatement.Expression = ExpectAndParseExpression(expressionStatement);
            ExpectEndOfStatement(expressionStatement);
            return Close(expressionStatement);
        }

        private ScriptReturnStatement ParseReturnStatement()
        {
            var ret = Open<ScriptReturnStatement>();
            NextToken(); // skip ret

            if (StartAsExpression())
            {
                ret.Expression = ParseExpression(ret);
            }
            ExpectEndOfStatement(ret);

            return Close(ret);
        }

        private ScriptRawStatement ParseRawStatement()
        {
            var scriptStatement = Open<ScriptRawStatement>();
            scriptStatement.Text = lexer.Text;
            NextToken(); // Skip raw
            return Close(scriptStatement);
        }

        private ScriptIfStatement ParseIfStatement()
        {
            // unit test: 200-if-else-statement.txt
            var condition = Open<ScriptIfStatement>();
            NextToken(); // skip if

            condition.Condition = ExpectAndParseExpression(condition);

            if (ExpectEndOfStatement(condition))
            {
                condition.Then = ParseBlockStatement(condition);
            }

            return Close(condition);
        }
        private ScriptWrapStatement ParseWrapStatement()
        {
            var wrapStatement = Open<ScriptWrapStatement>();
            NextToken(); // skip wrap

            wrapStatement.Target = ExpectAndParseExpression(wrapStatement);

            if (ExpectEndOfStatement(wrapStatement))
            {
                wrapStatement.Body = ParseBlockStatement(wrapStatement);
            }

            return Close(wrapStatement);
        }

        private ScriptExpression ExpectAndParseExpression(ScriptNode parentNode, ScriptExpression parentExpression = null, int newPrecedence = 0, string message = null)
        {
            if (StartAsExpression())
            {
                return ParseExpression(parentNode, parentExpression, newPrecedence);
            }
            LogError(parentNode, CurrentSpan, message ?? $"Expecting <expression> instead of [{Current.Type}]" );
            return null;
        }


        private ScriptExpression ExpectAndParseExpression(ScriptNode parentNode, ref bool hasAnonymousExpression, ScriptExpression parentExpression = null, int newPrecedence = 0, string message = null)
        {
            if (StartAsExpression())
            {
                return ParseExpression(parentNode, ref hasAnonymousExpression, parentExpression, newPrecedence);
            }
            LogError(parentNode, CurrentSpan, message ?? $"Expecting <expression> instead of [{Current.Type}]");
            return null;
        }


        private ScriptConditionStatement ParseElseStatement()
        {
            // unit test: 200-if-else-statement.txt
            var nextToken = PeekToken();
            if (nextToken.Type == TokenType.Identifier && GetAsText(nextToken) == "if")
            {
                NextToken();
                return ParseIfStatement();
            }

            var elseStatement = Open<ScriptElseStatement>();
            NextToken(); // skip else

            // unit test: 201-if-else-error4.txt
            if (ExpectEndOfStatement(elseStatement))
            {
                elseStatement.Body = ParseBlockStatement(elseStatement);
            }
            return Close(elseStatement);
        }

        private ScriptForStatement ParseForStatement()
        {
            var forStatement = Open<ScriptForStatement>();
            NextToken(); // skip for

            // unit test: 211-for-error1.txt
            forStatement.Variable = ExpectAndParseVariable(forStatement);

            if (forStatement.Variable != null)
            {
                // in 
                if (Current.Type != TokenType.Identifier || GetAsText(Current) != "in")
                {
                    // unit test: 211-for-error2.txt
                    LogError(forStatement, $"Expecting 'in' word instead of [{Current.Type} {GetAsText(Current)}]");
                }
                else
                {
                    NextToken(); // skip in
                }

                // unit test: 211-for-error3.txt
                forStatement.Iterator = ExpectAndParseExpression(forStatement);

                if (ExpectEndOfStatement(forStatement))
                {
                    forStatement.Body = ParseBlockStatement(forStatement);
                }
            }

            return Close(forStatement);
        }

        private ScriptWhileStatement ParseWhileStatement()
        {
            var whileStatement = Open<ScriptWhileStatement>();
            NextToken(); // Skip while

            // Parse the condition
            // unit test: 220-while-error1.txt
            whileStatement.Condition = ExpectAndParseExpression(whileStatement);

            if (ExpectEndOfStatement(whileStatement))
            {
                whileStatement.Body = ParseBlockStatement(whileStatement);
            }

            return Close(whileStatement);
        }

        private ScriptBlockStatement ParseBlockStatement(ScriptStatement parentStatement)
        {
            Blocks.Push(parentStatement);

            blockLevel++;
            var blockStatement = parentStatement is ScriptBlockStatement
                ? (ScriptBlockStatement) parentStatement
                : Open<ScriptBlockStatement>();

            ScriptStatement statement;
            bool hasEnd;
            while (TryParseStatement(parentStatement, out statement, out hasEnd))
            {
                // statement may be null if we have parsed an else continuation of a previous block
                if (statement != null)
                {
                    blockStatement.Statements.Add(statement);
                }
                if (hasEnd)
                {
                    break;
                }
            }

            if (!HasErrors && !hasEnd)
            {
                // If there are any end block not matching, we have an error
                if (blockLevel > 1)
                {
                    // unit test: 201-if-else-error2.txt
                    LogError(parentStatement, GetSpanForToken(Previous), $"The <end> statement was not found");
                }
            }

            blockLevel--;

            Blocks.Pop();
            return Close(blockStatement);
        }

        private bool IsInLoop()
        {
            foreach (var block in Blocks)
            {
                if (block is ScriptLoopStatementBase)
                {
                    return true;
                }
            }
            return false;
        }

        private T Open<T>() where T : ScriptNode, new()
        {
            return new T() { Span = {FileName = lexer.SourcePath, Start = Current.Start}};
        }

        private T Close<T>(T statement) where T : ScriptNode
        {
            statement.Span.End = Previous.End;
            return statement;
        }

        private string GetAsText(Token localToken)
        {
            return localToken.GetText(lexer.Text);
        }

        private void NextToken()
        {
            previousToken = token;
            bool result;

            if (tokensPreview.Count > 0)
            {
                token = tokensPreview.First.Value;
                tokensPreview.RemoveFirst();
                return;
            }

            // Skip Comments
            while ((result = tokenIt.MoveNext()) && IsHidden(tokenIt.Current.Type))
            {
            }

            token = result ? tokenIt.Current : Token.Eof;
        }

        private Token PeekToken(int count = 1)
        {
            if (count < 1) throw new ArgumentOutOfRangeException("count", "Must be > 0");

            for (int i = tokensPreview.Count; i < count; i++)
            {
                if (tokenIt.MoveNext())
                {
                    var nextToken = tokenIt.Current;
                    if (!IsHidden(nextToken.Type))
                    {
                        tokensPreview.AddLast(nextToken);
                    }
                }
            }

            if (tokensPreview.Count == 0)
            {
                return Token.Eof;
            }

            // optimized case for last element
            if (count >= tokensPreview.Count)
            {
                return tokensPreview.Last.Value;
            }

            // TODO: depending on the count it may be faster to start from the end of the list
            var node = tokensPreview.First;
            while (--count != 0)
            {
                node = node.Next;
            }
            return node.Value;
        }

        private bool IsHidden(TokenType tokenType)
        {
            return tokenType == TokenType.Comment || tokenType == TokenType.CommentMulti ||
                   (tokenType == TokenType.NewLine && allowNewLineLevel > 0);
        }

        private void LogError(string text)
        {
            LogError(Current, text);
        }

        private void LogError(Token tokenArg, string text)
        {
            LogError(GetSpanForToken(tokenArg), text);
        }

        private SourceSpan GetSpanForToken(Token tokenArg)
        {
            return new SourceSpan(lexer.SourcePath, tokenArg.Start, tokenArg.End);
        }

        private void LogError(SourceSpan span, string text)
        {
            Log(new LogMessage(ParserMessageType.Error, span, text));
        }

        private void LogError(ScriptNode node, string message)
        {
            LogError(node, node.Span, message);
        }

        private void LogError(ScriptNode node, SourceSpan span, string message)
        {
            var syntax = ScriptSyntaxAttribute.Get(node);
            string inMessage = " in";
            if (message.EndsWith("after"))
            {
                inMessage = string.Empty;
            }
            LogError(span, $"Error while parsing {syntax.Name}: {message}{inMessage}: {syntax.Example}");
        }

        private void Log(LogMessage logMessage)
        {
            if (logMessage == null) throw new ArgumentNullException(nameof(logMessage));
            Messages.Add(logMessage);
            if (logMessage.Type == ParserMessageType.Error)
            {
                HasErrors = true;
            }
        }

        private static bool IsKeyword(string text)
        {
            switch (text)
            {
                case "if":
                case "else":
                case "end":
                case "for":
                case "while":
                case "break":
                case "continue":
                case "func":
                case "import":
                case "readonly":
                case "with":
                case "capture":
                case "ret":
                case "wrap":
                case "do":
                    return true;
            }
            return false;
        }
    }
}