using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban.Parsing;

namespace Scriban.SourceGenerator
{
    static class Extensions
    {
        public static TextSpan ToTextSpan(this SourceSpan span)
        {
            var start = span.Start.Offset;
            var len = span.End.Offset - start;
            return new TextSpan(start, len);
        }

        public static LinePositionSpan ToLinePositionSpan(this SourceSpan span)
        {
            var start = span.Start;
            var end = span.End;
            return new LinePositionSpan(
                new LinePosition(start.Line, start.Column),
                new LinePosition(end.Line, end.Column)
            );
        }

        public static Location ToLocation(this SourceSpan span, string path)
        {
            return Location.Create(path, span.ToTextSpan(), span.ToLinePositionSpan());
        }
    }
}
