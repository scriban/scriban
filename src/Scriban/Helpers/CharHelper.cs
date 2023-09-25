using System;

namespace Scriban.Helpers
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class CharHelper
    {
        public static bool IsHexa(char c)
        {
            return char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }

        public static bool TryParseDigit(char c, out int value)
        {
            if (c >= '0' && c <= '9')
            {
                value = c - '0';
                return true;
            }
            value = 0;
            return false;
        }

        public static bool TryHexaToInt(char c, out int value)
        {
            switch (c)
            {
                case >= '0' and <= '9':
                    value = c - '0';
                    return true;
                case >= 'A' and <= 'F':
                    value = 10 + (c - 'A');
                    return true;
                case >= 'a' and <= 'f':
                    value = 10 + (c - 'a');
                    return true;
            }

            value = 0;
            return false;
        }

        public static bool IsBinary(char c) => c is '0' or '1';
    }
}