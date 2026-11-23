using Shop_API.Exceptions;
using System.Globalization;
using System.Text;

namespace Shop_API.Logics.Helpers
{
    public static class SlugHelper
    {
        private const int MaxAttempts = 1000;

        public static async Task<string> GenerateUniqueAsync(string? requestedSlug, string? name, Func<string, Task<bool>> slugExistsAsync, int maxLength = 200)
        {
            var source = string.IsNullOrWhiteSpace(requestedSlug) ? name : requestedSlug;

            var baseSlug = ToSlug(source, maxLength);

            if (baseSlug.Length == 0)
            {
                throw new ArgumentException("امکان ساخت نشانی (Slug) از مقدار واردشده وجود ندارد. لطفاً مقدار دیگری وارد کنید.");
            }

            if (!await slugExistsAsync(baseSlug))
            {
                return baseSlug;
            }

            for (var counter = 2; counter <= MaxAttempts; counter++)
            {
                var suffix = "-" + counter.ToString(CultureInfo.InvariantCulture);

                var availableLength = Math.Max(0, maxLength - suffix.Length);

                var prefix = baseSlug.Length > availableLength ? baseSlug[..availableLength].TrimEnd('-') : baseSlug;

                var candidate = prefix + suffix;

                if (!await slugExistsAsync(candidate))
                {
                    return candidate;
                }
            }

            throw new ConflictException("امکان ساخت نشانی (Slug) یکتا وجود ندارد. لطفاً نشانی دیگری وارد کنید.");
        }

        private static string ToSlug(string? text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text) || maxLength <= 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(text.Length);

            var lastWasDash = true;

            foreach (var character in text.Normalize(NormalizationForm.FormKC))
            {
                if (character is (>= '\u064B' and <= '\u065F') or '\u0670' or '\u0640')
                {
                    continue;
                }

                var mapped = character switch
                {
                    '\u064A' or '\u0649' => '\u06CC',
                    '\u0643' => '\u06A9',
                    >= '\u06F0' and <= '\u06F9' => (char)('0' + (character - '\u06F0')),
                    >= '\u0660' and <= '\u0669' => (char)('0' + (character - '\u0660')),
                    _ => char.ToLowerInvariant(character)
                };

                if (char.IsLetterOrDigit(mapped))
                {
                    builder.Append(mapped);

                    lastWasDash = false;

                    continue;
                }

                if (!lastWasDash)
                {
                    builder.Append('-');

                    lastWasDash = true;
                }
            }

            var slug = builder.ToString().TrimEnd('-');

            if (slug.Length > maxLength)
            {
                slug = slug[..maxLength].TrimEnd('-');
            }

            return slug;
        }
    }
}