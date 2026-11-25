using System.Security.Claims;
using Shop_API.Exceptions;

namespace Shop_API.Logics.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirst("sub")?.Value;

            if (!int.TryParse(value, out var id))
            {
                throw new UnauthorizedException("توکن نامعتبر است.");
            }

            return id;
        }
    }
}