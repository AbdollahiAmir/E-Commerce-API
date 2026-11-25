using Shop_API.DTOs.AuthDTOs;

namespace Shop_API.Logics.IServices.IAuthServices
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);

        Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);

        Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default);

        Task LogoutAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default);

        Task<MeDto> GetMeAsync(int userId, CancellationToken cancellationToken = default);
    }
}