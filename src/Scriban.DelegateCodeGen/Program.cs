using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Scriban.DelegateCodeGen
{
    /// <summary>
    /// Program generating pre-compiled custom delegates for all builtin functions
    /// Output result is in: `src/Scriban/CustomFunction.Generated.cs`
    /// </summary>
    class Program
    {
        private readonly AssemblyDefinition _assemblyDefinition;

        private readonly Dictionary<string, List<MethodDefinition>> _methods;
        private readonly string _repoRoot;

        public Program()
        {
            _repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
            var scribanAssemblyPath = Path.Combine(_repoRoot, "src", "Scriban", "bin", "Debug", "netstandard2.0", "Scriban.dll");
            _assemblyDefinition = AssemblyDefinition.ReadAssembly(scribanAssemblyPath);
            _methods = new Dictionary<string, List<MethodDefinition>>();
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

            var generatedFilePath = Path.Combine(_repoRoot, "src", "Scriban", "Runtime", "CustomFunction.Generated.cs");
            using var writer = new StreamWriter(generatedFilePath);
            writer.WriteLine("// ----------------------------------------------------------------------------------");
            writer.WriteLine($"// This file was automatically generated - {DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat)} by Scriban.DelegateCodeGen");
            writer.WriteLine("// DOT NOT EDIT THIS FILE MANUALLY");
            writer.WriteLine("// ----------------------------------------------------------------------------------");

            writer.WriteLine();

            writer.WriteLine(@"#nullable enable

using System;
using System.Collections;
using System.Reflection;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Syntax;

namespace Scriban.Runtime
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    abstract partial class DynamicCustomFunction
    {
");
            writer.Write(@"
        static DynamicCustomFunction()
        {
");
            foreach (var keyPair in _methods.OrderBy(s => s.Key))
            {
                foreach (var method in keyPair.Value)
                {
                    var name = "Function" + GetSignature(method, SignatureMode.Name);
                    var methodName = method.Name;
                    var parameterTypes = GetReflectionParameterTypes(method);
                    writer.WriteLine($@"            AddBuiltinFunctionDelegate(typeof({method.DeclaringType.FullName}).GetMethod(nameof({method.DeclaringType.FullName}.{methodName}), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly, null, {parameterTypes}, null), method => new {name}(method));");
                }
            }
            writer.Write(@"
        }
");

            foreach (var keyPair in _methods.OrderBy(s => s.Key))
            {
                DumpMethod(writer, keyPair.Value.First());
            }

            writer.WriteLine(@"
    }
}");
            writer.WriteLine();
        }

        private static void DumpMethod(TextWriter writer, MethodDefinition method)
        {
            var signature = GetSignature(method, SignatureMode.Verbose);
            var name = "Function" + GetSignature(method, SignatureMode.Name);
            var delegateSignature = GetSignature(method, SignatureMode.Delegate);

            var caseArgumentsBuilder = new StringBuilder();
            var delegateCallArgs = new StringBuilder();

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
                    delegateCallArgs.Append("callerSpan");
                    continue;
                }

                // If it is the last parameter and it is a params array, we need to handle it differently
                if (paramIndex + 1 == method.Parameters.Count && arg.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(ParamArrayAttribute).FullName))
                {
                    if (type.FullName == "System.Object[]")
                    {
                        caseArgumentsBuilder.AppendLine($"                var arg{argIndex}Array = arguments[{argIndex}] as ScriptArray ?? throw new ScriptRuntimeException(callerSpan, $\"Invalid params array for function `{{Method.Name}}`.\");");
                        caseArgumentsBuilder.AppendLine($"                object?[] arg{argIndex} = arg{argIndex}Array.ToArray();");
                    }
                    else
                    {
                        caseArgumentsBuilder.AppendLine($"                var arg{argIndex} = ({PrettyType(type)})arguments[{argIndex}];");
                    }
                }
                else
                {
                    caseArgumentsBuilder.AppendLine($"                var arg{argIndex} = {GetArgumentConversionMethod(arg)}<{PrettyType(type)}>(context, callerSpan, arguments[{argIndex}], {argIndex});");
                }

                delegateCallArgs.Append($"arg{argIndex}");
                argIndex++;
            }

            var template = $@"
        /// <summary>
        /// Optimized custom function for: {signature}
        /// </summary>
        private partial class {name} : DynamicCustomFunction
        {{
            private delegate {delegateSignature};

            private readonly InternalDelegate _delegate;

            public {name}(MethodInfo method) : base(method)
            {{
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }}

            public override object? Invoke(TemplateContext context, ScriptNode? callerContext, ScriptArray arguments, ScriptBlockStatement? blockStatement)
            {{
                var callerSpan = callerContext?.Span ?? context.CurrentSpan;
{caseArgumentsBuilder}
                return _delegate({delegateCallArgs});
            }}
        }}
";

            writer.Write(template);
        }

        private void CollectMethods(string type)
        {

            var typeDefinition = _assemblyDefinition.MainModule.GetType(type);
            if (typeDefinition is null)
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

                var signature = GetSignature(method, SignatureMode.Name);
                if (!_methods.ContainsKey(signature))
                {
                    _methods.Add(signature, new List<MethodDefinition>());
                }
                _methods[signature].Add(method);
            }
        }

        private static string GetSignature(MethodDefinition method, SignatureMode mode)
        {
            bool asCSharp = mode != SignatureMode.Name;

            var text = new StringBuilder();
            text.Append(PrettyType(method.ReturnType, asCSharp));
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

                if (mode == SignatureMode.Delegate && parameter.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(ParamArrayAttribute).FullName) && parameter.ParameterType.FullName == "System.Object[]")
                {
                    text.Append("object?[]");
                }
                else
                {
                    text.Append(PrettyType(parameter.ParameterType, asCSharp));
                    if (mode == SignatureMode.Delegate &&  IsNullableReferenceParameter(parameter))
                    {
                        text.Append("?");
                    }
                }

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

        private static string GetReflectionParameterTypes(MethodDefinition method)
        {
            if (method.Parameters.Count == 0)
            {
                return "Type.EmptyTypes";
            }

            var text = new StringBuilder("new[] { ");
            for (var i = 0; i < method.Parameters.Count; i++)
            {
                if (i > 0)
                {
                    text.Append(", ");
                }

                text.Append("typeof(");
                text.Append(PrettyType(method.Parameters[i].ParameterType));
                text.Append(")");
            }

            text.Append(" }");
            return text.ToString();
        }

        private enum SignatureMode
        {
            Verbose,

            Name,

            Delegate,
        }

        private static string GetArgumentConversionMethod(ParameterDefinition parameter)
        {
            if (parameter.ParameterType.IsValueType)
            {
                return "ConvertStructArgument";
            }

            return "ConvertNullableReferenceArgument";
        }

        private static bool  IsNullableReferenceParameter(ParameterDefinition parameter)
        {
            if (parameter.ParameterType.IsValueType || parameter.ParameterType.Name == "TemplateContext")
            {
                return false;
            }

            return true;
        }

        private static string PrettyType(TypeReference typeReference, bool asCSharp = true)
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

            if (!asCSharp && typeReference.IsArray)
            {
                return PrettyType(((ArrayType)typeReference).ElementType) + "Array";
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
