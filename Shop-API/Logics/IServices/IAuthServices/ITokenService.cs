namespace Shop_API.Logics.IServices.IAuthServices
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) CreateAccessToken(int userId);

        (string Token, byte[] Hash, DateTime ExpiresAt) CreateRefreshToken();

        byte[] HashRefreshToken(string token);
    }
}