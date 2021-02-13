using System.Threading.Tasks;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.Tests
{
    class CustomTemplateLoader : ITemplateLoader
    {
        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            if (templateName == "null")
            {
                return null;
            }

            return templateName;
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            switch (templatePath)
            {
                case "invalid":
                    return "Invalid script with syntax error: {{ 1 + }}";

                case "invalid2":
                    return null;

                case "arguments":
                    return "{{ $1 }} + {{ $2 }}";

                case "product":
                    return "product: {{ product.title }}";

                case "nested_templates":
                    return "{{ include 'header' }} {{ include 'body' }} {{ include 'footer' }}";

                case "body":
                    return "body {{ include 'body_detail' }}";

                case "header":
                    return "This is a header";

                case "body_detail":
                    return "This is a body_detail";

                case "footer":
                    return "This is a footer";

                case "recursive_nested_templates":
                    return "{{$1}}{{ x = x ?? 0; x = x + 1; if x < 5; include 'recursive_nested_templates' ($1 + 1); end }}";

                case "multilines":
                    return "Line 1\nLine 2\nLine 3";

                case "nested_templates_with_indent":
                    return "  {{ include 'multilines'}}";

                case "named_arguments":
                    return "{{ $.this_arg }}";

                default:
                    return templatePath;
            }
        }

        public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return new ValueTask<string>(Load(context, callerSpan, templatePath));
        }
    }

    class LiquidCustomTemplateLoader : ITemplateLoader
    {
        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            return templateName;
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            switch (templatePath)
            {
                case "arguments":
                    return "{{ var1 }} + {{ var2 }}";

                case "with_arguments":
                    return "{{ with_arguments }} : {{ var1 }} + {{ var2 }}";

                case "with_product":
                    return "with_product: {{ with_product.title }}";

                case "for_product":
                    return "for_product: {{ for_product.title }} ";

                default:
                    return templatePath;
            }
        }

        public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return new ValueTask<string>(Load(context, callerSpan, templatePath));
        }
    }
}