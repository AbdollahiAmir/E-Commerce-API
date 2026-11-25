using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Shop_API.Constants;
using Shop_API.Data;
using Shop_API.Logics.IServices.IAuthServices;

namespace Shop_API.Logics.Services.AuthServices
{
    public class UserAccessService : IUserAccessService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        private readonly IMemoryCache _cache;
        private readonly IServiceScopeFactory _scopeFactory;

        private CancellationTokenSource _resetSource = new();

        public UserAccessService(IMemoryCache cache, IServiceScopeFactory scopeFactory)
        {
            _cache = cache;
            _scopeFactory = scopeFactory;
        }

        private static string BuildKey(int userId)
        {
            return $"user-access:{userId}";
        }

        public async Task<UserAccess?> GetAsync(int userId, CancellationToken cancellationToken = default)
        {
            var key = BuildKey(userId);

            var lazy = _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                entry.AddExpirationToken(new CancellationChangeToken(Volatile.Read(ref _resetSource).Token));

                return new Lazy<Task<UserAccess?>>(() => LoadAsync(userId), LazyThreadSafetyMode.ExecutionAndPublication);
            })!;

            try
            {
                return await lazy.Value.WaitAsync(cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _cache.Remove(key);

                throw;
            }
        }

        public void Invalidate(int userId)
        {
            _cache.Remove(BuildKey(userId));
        }

        public void InvalidateAll()
        {
            var previous = Interlocked.Exchange(ref _resetSource, new CancellationTokenSource());

            previous.Cancel();
        }

        private async Task<UserAccess?> LoadAsync(int userId)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var isActive = await context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => (bool?)u.IsActive)
                .FirstOrDefaultAsync();

            if (isActive == null)
            {
                return null;
            }

            var level = await (from ur in context.UserRoles
                               join r in context.Roles on ur.RoleId equals r.Id
                               where ur.UserId == userId
                               select (int?)r.Level).MaxAsync() ?? 0;

            var permissions = await (from ur in context.UserRoles
                                     join c in context.RoleClaims on ur.RoleId equals c.RoleId
                                     where ur.UserId == userId && c.ClaimType == Permissions.ClaimType
                                     select c.ClaimValue!).Distinct().ToListAsync();

            return new UserAccess(isActive.Value, level, permissions.ToHashSet());
        }
    }
}