using Microsoft.AspNetCore.Authorization;

namespace Shop_API.Authorization
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public const string PolicyPrefix = "perm:";

        public HasPermissionAttribute(string permission)
        {
            Policy = PolicyPrefix + permission;
        }
    }
}