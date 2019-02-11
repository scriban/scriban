using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Scriban.CodeGen
{
    /// <summary>
    /// Program generating pre-compiled custom delegates for all builtin functions
    /// Output result is in: `src/Scriban/CustomFunction.Generated.cs`
    /// </summary>
    class Program
    {
        private readonly AssemblyDefinition _assemblyDefinition;

        private readonly Dictionary<string, MethodDefinition> _methods;

        private TextWriter _writer;

        public Program()
        {
            _assemblyDefinition = AssemblyDefinition.ReadAssembly(@"..\..\..\..\Scriban\bin\Debug\net40\Scriban.dll");
            _methods = new Dictionary<string, MethodDefinition>();
        }

        public void Run()
        {
            CollectMethods("Scriban.Functions.ArrayFunctions");
            CollectMethods("Scriban.Functions.DateTimeFunctions");
            CollectMethods("Scriban.Functions.HtmlFunctions");
            CollectMethods("Scriban.Functions.MathFunctions");
            CollectMethods("Scriban.Functions.ObjectFunctions");
            CollectMethods("Scriban.Functions.RegexFunctions");
            CollectMethods("Scriban.Functions.StringFunctions");
            CollectMethods("Scriban.Functions.TimeSpanFunctions");

            _writer = new StreamWriter(@"..\..\..\..\Scriban\Runtime\CustomFunction.Generated.cs");
            _writer.WriteLine("// ----------------------------------------------------------------------------------");
            _writer.WriteLine($"// This file was automatically generated - {DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat)} by Scriban.CodeGen");
            _writer.WriteLine("// DOT NOT EDIT THIS FILE MANUALLY");
            _writer.WriteLine("// ----------------------------------------------------------------------------------");

            _writer.WriteLine();

            _writer.WriteLine(@"using System;
using System.Collections;
using System.Reflection;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Syntax;

namespace Scriban.Runtime
{
    public abstract partial class DynamicCustomFunction
    {
");
            _writer.Write(@"
        static DynamicCustomFunction()
        {
");
            foreach (var keyPair in _methods.OrderBy(s => s.Key))
            {
                var method = keyPair.Value;
                var name = "Function" + GetSignature(method, SignatureMode.Name);

                var methodName = method.Name;
                _writer.WriteLine($@"            BuiltinFunctionDelegates.Add(typeof({method.DeclaringType.FullName}).GetTypeInfo().GetDeclaredMethod(nameof({method.DeclaringType.FullName}.{methodName})), method => new {name}(method));");
            }
            _writer.Write(@"
        }
");

            foreach (var keyPair in _methods.OrderBy(s => s.Key))
            {
                DumpMethod(keyPair.Key, keyPair.Value);
            }

            _writer.WriteLine(@"
    }
}");
            _writer.WriteLine();
            _writer.Flush();
            _writer.Close();
        }

        private void DumpMethod(string signature, MethodDefinition method)
        {
            var name = "Function" + GetSignature(method, SignatureMode.Name);
            var delegateSignature = GetSignature(method, SignatureMode.Delegate);

            var arguments = new StringBuilder();
            var caseArguments = "";
            var caseArgumentsBuilder = new StringBuilder();
            var delegateCallArgs = new StringBuilder();
            var defaultParamDeclaration = new StringBuilder();
            var defaultParamConstructors = new StringBuilder();

            int argumentCount = 0;
            int argOffset = 0;
            for (var i = 0; i < method.Parameters.Count; i++)
            {
                var arg = method.Parameters[i];
                var type = arg.ParameterType;
                if (type.Name == "TemplateContext" || type.Name == "SourceSpan")
                {
                    argOffset++;
                    continue;
                }
                argumentCount++;
            }
            int minimunArg = argumentCount;

            string argCheckMask = $"argMask != (1 << {argumentCount}) - 1";

            int argIndex = 0;
            for (var paramIndex = 0; paramIndex < method.Parameters.Count; paramIndex++)
            {
                var arg = method.Parameters[paramIndex];
                var type = arg.ParameterType;

                if (paramIndex > 0)
                {
                    delegateCallArgs.Append(", ");
                }

                if (type.Name == "TemplateContext")
                {
                    delegateCallArgs.Append("context");
                    continue;
                }
                if (type.Name == "SourceSpan")
                {
                    delegateCallArgs.Append("callerContext.Span");
                    continue;
                }

                arguments.Append($"                var arg{argIndex} = ");
                caseArgumentsBuilder.AppendLine($"                        case {argIndex}:");
                caseArgumentsBuilder.Append($"                            arg{argIndex} = ");

                if (arg.IsOptional)
                {
                    if (argIndex < minimunArg)
                    {
                        minimunArg = argIndex;
                    }
                    arguments.Append($"defaultArg{argIndex}");
                }
                else
                {
                    arguments.Append($"default({PrettyType(type)})");
                }
                arguments.AppendLine(";");

                if (type.MetadataType == MetadataType.String)
                {
                    caseArgumentsBuilder.Append($"context.ToString(callerContext.Span, arg)");
                }
                else if (type.MetadataType == MetadataType.Int32)
                {
                    caseArgumentsBuilder.Append($"context.ToInt(callerContext.Span, arg)");
                }
                else
                {
                    if (type.MetadataType != MetadataType.Object)
                    {
                        if (type.Name == "IList")
                        {
                            caseArgumentsBuilder.Append($"context.ToList(callerContext.Span, arg)");
                        }
                        else
                        {
                            caseArgumentsBuilder.Append($"({PrettyType(type)})context.ToObject(callerContext.Span, arg, typeof({PrettyType(type)}))");
                        }
                    }
                    else
                    {
                        caseArgumentsBuilder.Append($"arg");
                    }
                }
                caseArgumentsBuilder.AppendLine(";");

                // If argument is optional, we don't need to update the mask as it is already taken into account into the mask init
                if (!arg.IsOptional)
                {
                    caseArgumentsBuilder.AppendLine($"                            argMask |= (1 << {argIndex});");
                }
                caseArgumentsBuilder.AppendLine($"                            break;");

                delegateCallArgs.Append($"arg{argIndex}");
                argIndex++;
            }

            if (method.Parameters.Count != 0)
            {
                caseArguments = $@"
                    switch (argIndex)
                    {{
{caseArgumentsBuilder}
                    }}";
            }

            // Output default argument masking
            var defaultArgMask = 0;
            for (int i = minimunArg; i < argumentCount; i++)
            {
                defaultArgMask |= (1 << i);
            }
            arguments.AppendLine($"                int argMask = {defaultArgMask};");

            var argCheck = $"arguments.Count";
            string atLeast = "";
            if (minimunArg != argumentCount)
            {
                atLeast = "at least ";
                argCheck += " < " + minimunArg;
                argCheck += " || arguments.Count > " + argumentCount;

                for (var i = 0; i < method.Parameters.Count; i++)
                {
                    var arg = method.Parameters[i];

                    if (arg.IsOptional)
                    {
                        argIndex = i - argOffset;
                        defaultParamDeclaration.AppendLine();
                        defaultParamDeclaration.Append($"            private readonly {PrettyType(arg.ParameterType)} defaultArg{argIndex};");
                        defaultParamConstructors.AppendLine();
                        defaultParamConstructors.Append($"                defaultArg{argIndex} = ({PrettyType(arg.ParameterType)})Parameters[{i}].DefaultValue;");
                    }
                }
            }
            else
            {
                argCheck += " != " + argumentCount;
            }


            var template = $@"
        /// <summary>
        /// Optimized custom function for: {signature}
        /// </summary>
        private partial class {name} : DynamicCustomFunction
        {{
            private delegate {delegateSignature};

            private readonly InternalDelegate _delegate;{defaultParamDeclaration}

            public {name}(MethodInfo method) : base(method)
            {{
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));{defaultParamConstructors}
            }}

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {{
                if ({argCheck})
                {{
                    throw new ScriptRuntimeException(callerContext.Span, $""Invalid number of arguments `{{arguments.Count}}` passed to `{{callerContext}}` while expecting {atLeast}`{minimunArg}` arguments"");
                }}
{arguments}
                int argOrderedIndex = 0;
                for (int i = 0; i < arguments.Count; i++)
                {{
                    int argIndex = 0;
                    var arg = arguments[i];
                    var namedArg = arg as ScriptNamedArgument;
                    if (namedArg != null)
                    {{
                        var namedArgValue = GetValueFromNamedArgument(context, callerContext, namedArg);
                        arg = namedArgValue.Value;
                        argIndex = namedArgValue.Index - {argOffset};
                    }}
                    else
                    {{
                        argIndex = argOrderedIndex;
                        argOrderedIndex++;
                    }}
{caseArguments}
                }}

                if ({argCheckMask})
                {{
                    throw new ScriptRuntimeException(callerContext.Span, $""Invalid number of arguments `{{arguments.Count}}` passed to `{{callerContext}}` while expecting {atLeast}`{minimunArg}` arguments"");
                }}

                return _delegate({delegateCallArgs});
            }}
        }}
";

            _writer.Write(template);
        }

        private void CollectMethods(string type)
        {

            var typeDefinition = _assemblyDefinition.MainModule.GetType(type);
            if (typeDefinition == null)
            {
                throw new InvalidOperationException($"Unable to find type {type}");
            }


            foreach (var method in typeDefinition.Methods)
            {
                if (method.IsConstructor || !method.IsPublic || !method.IsStatic || method.IsGetter)
                {
                    continue;
                }

                if (method.Parameters.Any(p => p.ParameterType.IsGenericInstance) || method.ReturnType.IsGenericInstance)
                {
                    continue;
                }

                if (method.CustomAttributes.Any(attr => attr.Constructor.Name == "ScriptMemberIgnore"))
                {
                    continue;
                }

                var signature = GetSignature(method, SignatureMode.Verbose);
                if (!_methods.ContainsKey(signature))
                {
                    _methods.Add(signature, method);
                }
            }
        }

        private static string GetSignature(MethodDefinition method, SignatureMode mode)
        {
            var text = new StringBuilder();
            text.Append(PrettyType(method.ReturnType));
            if (mode == SignatureMode.Verbose)
            {
                text.Append(" (");
            }
            else if (mode == SignatureMode.Delegate)
            {
                text.Append(" InternalDelegate(");
            }
            for (var i = 0; i < method.Parameters.Count; i++)
            {
                var parameter = method.Parameters[i];
                if (i > 0)
                {
                    if (mode != SignatureMode.Name)
                    {
                        text.Append(", ");
                    }
                }

                if (mode == SignatureMode.Name)
                {
                    text.Append("_");
                }

                text.Append(PrettyType(parameter.ParameterType));

                if (mode == SignatureMode.Delegate)
                {
                    text.Append($" arg{i}");
                }
                if (parameter.IsOptional)
                {
                    if (mode == SignatureMode.Verbose)
                    {
                        text.Append(" = ...");
                    }
                    else if (mode == SignatureMode.Name)
                    {
                        text.Append("___Opt");
                    }
                }
                // TODO: Handle params
            }
            if (mode != SignatureMode.Name)
            {
                text.Append(")");
            }

            return text.ToString();
        }

        private enum SignatureMode
        {
            Verbose,

            Name,

            Delegate,
        }



        private static string PrettyType(TypeReference typeReference)
        {
            switch (typeReference.MetadataType)
            {
                case MetadataType.String:
                    return "string";
                case MetadataType.Double:
                    return "double";
                case MetadataType.Single:
                    return "float";
                case MetadataType.Byte:
                    return "byte";
                case MetadataType.SByte:
                    return "sbyte";
                case MetadataType.Int16:
                    return "short";
                case MetadataType.UInt16:
                    return "ushort";
                case MetadataType.Int32:
                    return "int";
                case MetadataType.UInt32:
                    return "uint";
                case MetadataType.Int64:
                    return "long";
                case MetadataType.UInt64:
                    return "ulong";
                case MetadataType.Boolean:
                    return "bool";
                case MetadataType.Object:
                    return "object";
                case MetadataType.Void:
                    return "void";
            }

            return typeReference.Namespace.StartsWith("Scriban") || typeReference.Namespace == "System" || typeReference.Namespace == "System.Collections"
                ? typeReference.Name
                : typeReference.FullName;
        }

        static void Main()
        {
            var program = new Program();
            program.Run();
        }
    }
}
