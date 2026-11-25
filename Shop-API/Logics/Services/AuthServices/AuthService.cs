using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shop_API.Constants;
using Shop_API.Data;
using Shop_API.DTOs.AuthDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.Helpers;
using Shop_API.Logics.IServices.IAuthServices;
using Shop_API.Models.AuthModels;

namespace Shop_API.Logics.Services.AuthServices
{
    public class AuthService : IAuthService
    {
        private const string InvalidCredentialsMessage = "شماره موبایل یا رمز عبور اشتباه است.";
        private const string InvalidSessionMessage = "نشست شما معتبر نیست. لطفاً دوباره وارد شوید.";
        private const string InactiveMessage = "حساب کاربری شما غیرفعال شده است.";
        private const string InvalidPhoneMessage = "شماره موبایل معتبر نیست.";
        private const string DuplicatePhoneMessage = "این شماره موبایل قبلاً ثبت شده است.";
        private const int RefreshReuseGraceSeconds = 10;

        private static readonly ApplicationUser DummyUser = new();
        private static readonly string DummyHash = new PasswordHasher<ApplicationUser>().HashPassword(DummyUser, "Dummy-Password-1");

        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthService(AppDbContext context, UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _context = context;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        private (RefreshToken Entity, AuthResponseDto Response) BuildSession(int userId)
        {
            var access = _tokenService.CreateAccessToken(userId);

            var refresh = _tokenService.CreateRefreshToken();

            var entity = new RefreshToken
            {
                UserId = userId,
                TokenHash = refresh.Hash,
                ExpiresAt = refresh.ExpiresAt
            };

            var response = new AuthResponseDto
            {
                AccessToken = access.Token,
                AccessTokenExpiresAt = access.ExpiresAt,
                RefreshToken = refresh.Token,
                RefreshTokenExpiresAt = refresh.ExpiresAt
            };

            return (entity, response);
        }

        private static void ThrowIdentityErrors(IdentityResult result)
        {
            if (result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateUserName)))
            {
                throw new ConflictException(DuplicatePhoneMessage);
            }

            throw new ArgumentException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
        {
            if (!PhoneNumberHelper.TryNormalize(dto.PhoneNumber, out var phone))
            {
                throw new ArgumentException(InvalidPhoneMessage);
            }

            var fullName = dto.FullName.Trim();

            if (fullName.Length < 2)
            {
                throw new ArgumentException("نام و نام خانوادگی باید حداقل ۲ کاراکتر باشد.");
            }

            var user = new ApplicationUser
            {
                UserName = phone,
                PhoneNumber = phone,
                FullName = fullName
            };

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            IdentityResult result;

            try
            {
                result = await _userManager.CreateAsync(user, dto.Password);

                if (result.Succeeded)
                {
                    result = await _userManager.AddToRoleAsync(user, RoleNames.Customer);
                }
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 2601 or 2627 })
            {
                throw new ConflictException(DuplicatePhoneMessage);
            }

            if (!result.Succeeded)
            {
                ThrowIdentityErrors(result);
            }

            var (entity, response) = BuildSession(user.Id);

            _context.RefreshTokens.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return response;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
        {
            if (!PhoneNumberHelper.TryNormalize(dto.PhoneNumber, out var phone))
            {
                throw new ArgumentException(InvalidPhoneMessage);
            }

            var user = await _userManager.FindByNameAsync(phone);

            if (user == null)
            {
                _userManager.PasswordHasher.VerifyHashedPassword(DummyUser, DummyHash, dto.Password);

                throw new UnauthorizedException(InvalidCredentialsMessage);
            }

            var now = DateTimeOffset.UtcNow;

            if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > now)
            {
                var minutes = (int)Math.Ceiling((user.LockoutEnd.Value - now).TotalMinutes);

                throw new LockedException($"به‌دلیل تلاش‌های ناموفق زیاد، حساب شما موقتاً قفل شده است. {minutes} دقیقه‌ی دیگر دوباره تلاش کنید.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!passwordValid)
            {
                await _userManager.AccessFailedAsync(user);

                throw new UnauthorizedException(InvalidCredentialsMessage);
            }

            if (!user.IsActive)
            {
                throw new ForbiddenException(InactiveMessage);
            }

            if (user.AccessFailedCount > 0)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
            }

            var (entity, response) = BuildSession(user.Id);

            _context.RefreshTokens.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return response;
        }

        public async Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default)
        {
            var hash = _tokenService.HashRefreshToken(dto.RefreshToken);

            var now = DateTime.UtcNow;

            var stored = await _context.RefreshTokens
                .AsNoTracking()
                .Where(t => t.TokenHash == hash)
                .Select(t => new
                {
                    t.Id,
                    t.UserId,
                    t.ExpiresAt,
                    t.RevokedAt,
                    t.User.IsActive
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (stored == null)
            {
                throw new UnauthorizedException(InvalidSessionMessage);
            }

            if (stored.RevokedAt.HasValue)
            {
                if (stored.RevokedAt.Value < now.AddSeconds(-RefreshReuseGraceSeconds))
                {
                    await _context.RefreshTokens
                        .Where(t => t.UserId == stored.UserId && t.RevokedAt == null)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAt, now), cancellationToken);
                }

                throw new UnauthorizedException(InvalidSessionMessage);
            }

            if (stored.ExpiresAt <= now)
            {
                throw new UnauthorizedException(InvalidSessionMessage);
            }

            if (!stored.IsActive)
            {
                throw new ForbiddenException(InactiveMessage);
            }

            var (entity, response) = BuildSession(stored.UserId);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            _context.RefreshTokens.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            var revoked = await _context.RefreshTokens
                .Where(t => t.Id == stored.Id && t.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.RevokedAt, now)
                    .SetProperty(t => t.ReplacedByTokenId, entity.Id), cancellationToken);

            if (revoked == 0)
            {
                throw new UnauthorizedException(InvalidSessionMessage);
            }

            await transaction.CommitAsync(cancellationToken);

            return response;
        }

        public async Task LogoutAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default)
        {
            var hash = _tokenService.HashRefreshToken(dto.RefreshToken);

            var now = DateTime.UtcNow;

            await _context.RefreshTokens
                .Where(t => t.TokenHash == hash && t.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAt, now), cancellationToken);
        }

        public async Task<MeDto> GetMeAsync(int userId, CancellationToken cancellationToken = default)
        {
            var me = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    Dto = new MeDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        PhoneNumber = u.PhoneNumber!,
                        CreatedAt = u.CreatedAt,
                        Roles = (from ur in _context.UserRoles
                                 join r in _context.Roles on ur.RoleId equals r.Id
                                 where ur.UserId == u.Id
                                 select r.Name!).ToList()
                    },
                    u.IsActive
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (me == null)
            {
                throw new UnauthorizedException(InvalidSessionMessage);
            }

            if (!me.IsActive)
            {
                throw new ForbiddenException(InactiveMessage);
            }

            return me.Dto;
        }
    }
}