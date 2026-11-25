namespace Shop_API.Logics.Helpers
{
    public static class PhoneNumberHelper
    {
        public static bool TryNormalize(string? input, out string normalized)
        {
            normalized = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            Span<char> buffer = stackalloc char[16];

            var length = 0;

            foreach (var c in input)
            {
                char digit;

                if (c >= '0' && c <= '9')
                {
                    digit = c;
                }
                else if (c >= '۰' && c <= '۹')
                {
                    digit = (char)('0' + (c - '۰'));
                }
                else if (c >= '٠' && c <= '٩')
                {
                    digit = (char)('0' + (c - '٠'));
                }
                else if (char.IsWhiteSpace(c) || c is '-' or '(' or ')' || (c == '+' && length == 0))
                {
                    continue;
                }
                else
                {
                    return false;
                }

                if (length == buffer.Length)
                {
                    return false;
                }

                buffer[length++] = digit;
            }

            ReadOnlySpan<char> digits = buffer[..length];

            if (digits.StartsWith("0098".AsSpan()))
            {
                digits = digits[4..];
            }
            else if (digits.StartsWith("98".AsSpan()))
            {
                digits = digits[2..];
            }
            else if (digits.StartsWith("0".AsSpan()))
            {
                digits = digits[1..];
            }

            if (digits.Length != 10 || digits[0] != '9')
            {
                return false;
            }

            normalized = string.Concat("0".AsSpan(), digits);

            return true;
        }
    }
}