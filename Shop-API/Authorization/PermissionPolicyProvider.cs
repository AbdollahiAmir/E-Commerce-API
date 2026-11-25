using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Shop_API.Authorization
{
    public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly ConcurrentDictionary<string, AuthorizationPolicy> _policies = new();

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
        }

        public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (!policyName.StartsWith(HasPermissionAttribute.PolicyPrefix, StringComparison.Ordinal))
            {
                return base.GetPolicyAsync(policyName);
            }

            var policy = _policies.GetOrAdd(policyName, name =>
                new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(name[HasPermissionAttribute.PolicyPrefix.Length..]))
                    .Build());

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }
    }
}