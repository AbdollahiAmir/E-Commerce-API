using Microsoft.AspNetCore.Authorization;
using Shop_API.Logics.IServices.IAuthServices;

namespace Shop_API.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IUserAccessService _userAccessService;

        public PermissionAuthorizationHandler(IUserAccessService userAccessService)
        {
            _userAccessService = userAccessService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (!int.TryParse(context.User.FindFirst("sub")?.Value, out var userId))
            {
                return;
            }

            var access = await _userAccessService.GetAsync(userId);

            if (access is { IsActive: true } && access.Permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}