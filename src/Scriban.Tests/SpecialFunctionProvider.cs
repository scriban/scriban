using System.Text;

namespace Scriban.Tests
{
    public static class SpecialFunctionProvider
    {
        public static string SpecialConcatStrings(string a = "default_a", string b = "default_b", string c = "default_c")
        {
            return "a: " + (a ?? string.Empty) + ", b: " + (b ?? string.Empty) + ", c: " + (c ?? string.Empty);
        }

        public static string SpecialConcatStringAndParams(string a, params object[] args)
        {
            var builder = new StringBuilder();
            builder.Append("a: " + (a ?? string.Empty));
            for (int i = 0; i < args.Length; i++)
            {
                builder.Append($", v{i}: " + (args[i] ?? string.Empty));
            }
            return builder.ToString();
        }

        public static string SpecialConcatParams(params object[] args)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }
                builder.Append($"v{i}: " + (args[i] ?? string.Empty));
            }
            return builder.ToString();
        }
    }
}