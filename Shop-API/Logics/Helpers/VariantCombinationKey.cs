using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Shop_API.Logics.Helpers
{
    public static class VariantCombinationKey
    {
        public const int Length = 32;

        public static byte[] Build(IEnumerable<(int AttributeId, string Value)> items)
        {
            var builder = new StringBuilder();

            var first = true;

            foreach (var item in items.OrderBy(i => i.AttributeId))
            {
                if (!first)
                {
                    builder.Append('|');
                }

                first = false;

                var value = item.Value.ToUpperInvariant();

                builder.Append(item.AttributeId.ToString(CultureInfo.InvariantCulture));
                builder.Append(':');
                builder.Append(value.Length.ToString(CultureInfo.InvariantCulture));
                builder.Append(':');
                builder.Append(value);
            }

            return SHA256.HashData(Encoding.Unicode.GetBytes(builder.ToString()));
        }
    }
}