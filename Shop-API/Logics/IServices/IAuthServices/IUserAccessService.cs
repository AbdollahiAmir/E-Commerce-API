namespace Shop_API.Logics.IServices.IAuthServices
{
    public sealed record UserAccess(bool IsActive, int Level, HashSet<string> Permissions);

    public interface IUserAccessService
    {
        Task<UserAccess?> GetAsync(int userId, CancellationToken cancellationToken = default);

        void Invalidate(int userId);

        void InvalidateAll();
    }
}