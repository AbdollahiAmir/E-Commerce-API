using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shop_API.Data;
using Shop_API.DTOs.MediaDTOs;
using Shop_API.DTOs.UserDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.IServices.IAuthServices;

namespace Shop_API.Logics.Services.AuthServices
{
    public class UserService : IUserService
    {
        private const int MaxPageSize = 100;
        private const string InvalidAccessMessage = "دسترسی شما معتبر نیست.";
        private const string SelfMessage = "شما نمی‌توانید حساب یا نقش‌های خودتان را از این بخش تغییر دهید.";
        private const string HigherUserMessage = "شما اجازه‌ی مدیریت کاربری با سطح برابر یا بالاتر از سطح خود را ندارید.";

        private readonly AppDbContext _context;
        private readonly IUserAccessService _userAccessService;

        public UserService(AppDbContext context, IUserAccessService userAccessService)
        {
            _context = context;
            _userAccessService = userAccessService;
        }

        private async Task<UserAccess> GetActorAsync(int actorId, CancellationToken cancellationToken)
        {
            var access = await _userAccessService.GetAsync(actorId, cancellationToken);

            if (access is not { IsActive: true })
            {
                throw new ForbiddenException(InvalidAccessMessage);
            }

            return access;
        }

        private static string ToAsciiDigits(string value)
        {
            return string.Create(value.Length, value, (span, source) =>
            {
                for (var i = 0; i < source.Length; i++)
                {
                    var c = source[i];

                    span[i] = c is >= '۰' and <= '۹'
                        ? (char)('0' + (c - '۰'))
                        : c is >= '٠' and <= '٩'
                            ? (char)('0' + (c - '٠'))
                            : c;
                }
            });
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("این کاربر هم‌زمان توسط عملیات دیگری تغییر کرده است. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 2601 or 2627 })
            {
                throw new ConflictException("این نقش قبلاً برای کاربر ثبت شده است. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 547 })
            {
                throw new ConflictException("نقش یا کاربر انتخاب‌شده هم‌زمان حذف شد. لطفاً دوباره تلاش کنید.");
            }
        }

        private async Task LockUserAsync(int id, CancellationToken cancellationToken)
        {
            var locked = await _context.Database
                .SqlQuery<int>($"SELECT [Id] AS [Value] FROM [Users] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {id}")
                .ToListAsync(cancellationToken);

            if (locked.Count == 0)
            {
                throw new NotFoundException("کاربر مورد نظر یافت نشد.");
            }
        }

        private async Task EnsureCanManageAsync(UserAccess actor, int actorId, int targetId, CancellationToken cancellationToken)
        {
            if (targetId == actorId)
            {
                throw new ForbiddenException(SelfMessage);
            }

            var targetLevel = await (from ur in _context.UserRoles
                                     join r in _context.Roles on ur.RoleId equals r.Id
                                     where ur.UserId == targetId
                                     select (int?)r.Level).MaxAsync(cancellationToken) ?? 0;

            if (targetLevel >= actor.Level)
            {
                throw new ForbiddenException(HigherUserMessage);
            }
        }

        private async Task<UserDto?> LoadUserAsync(UserAccess actor, int actorId, int id, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber!,
                    PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Roles = (from ur in _context.UserRoles
                             join r in _context.Roles on ur.RoleId equals r.Id
                             where ur.UserId == u.Id
                             orderby r.Level descending, r.Name
                             select new UserRoleDto
                             {
                                 Id = r.Id,
                                 Name = r.Name!,
                                 DisplayName = r.DisplayName,
                                 Level = r.Level
                             }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user != null)
            {
                user.Level = user.Roles.Count == 0 ? 0 : user.Roles.Max(r => r.Level);

                user.CanManage = user.Id != actorId && user.Level < actor.Level;
            }

            return user;
        }

        public async Task<PagedResult<UserListDto>> GetPagedAsync(int actorId, int page, int pageSize, string? search, bool? isActive, int? roleId, CancellationToken cancellationToken = default)
        {
            var actor = await GetActorAsync(actorId, cancellationToken);

            if (page < 1)
            {
                throw new ArgumentException("شماره صفحه باید بزرگ‌تر یا مساوی ۱ باشد.");
            }

            if (pageSize < 1 || pageSize > MaxPageSize)
            {
                throw new ArgumentException($"تعداد آیتم‌های هر صفحه باید بین ۱ تا {MaxPageSize} باشد.");
            }

            var skip = (long)(page - 1) * pageSize;

            if (skip > int.MaxValue)
            {
                throw new ArgumentException("شماره صفحه معتبر نیست.");
            }

            var query = _context.Users.AsNoTracking();

            var term = search?.Trim();

            if (!string.IsNullOrEmpty(term))
            {
                var digits = ToAsciiDigits(term);

                query = query.Where(u => u.FullName.Contains(term) || u.PhoneNumber!.StartsWith(digits));
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (roleId.HasValue)
            {
                query = query.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == roleId.Value));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .ThenByDescending(u => u.Id)
                .Skip((int)skip)
                .Take(pageSize)
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber!,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    Roles = (from ur in _context.UserRoles
                             join r in _context.Roles on ur.RoleId equals r.Id
                             where ur.UserId == u.Id
                             orderby r.Level descending, r.Name
                             select new UserRoleDto
                             {
                                 Id = r.Id,
                                 Name = r.Name!,
                                 DisplayName = r.DisplayName,
                                 Level = r.Level
                             }).ToList()
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.Level = item.Roles.Count == 0 ? 0 : item.Roles.Max(r => r.Level);

                item.CanManage = item.Id != actorId && item.Level < actor.Level;
            }

            return new PagedResult<UserListDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<UserDto?> GetByIdAsync(int actorId, int id, CancellationToken cancellationToken = default)
        {
            var actor = await GetActorAsync(actorId, cancellationToken);

            return await LoadUserAsync(actor, actorId, id, cancellationToken);
        }

        public async Task<UserDto> EditAsync(int actorId, int id, EditUserDto dto, CancellationToken cancellationToken = default)
        {
            var actor = await GetActorAsync(actorId, cancellationToken);

            if (id == actorId)
            {
                throw new ForbiddenException(SelfMessage);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            await LockUserAsync(id, cancellationToken);

            await EnsureCanManageAsync(actor, actorId, id, cancellationToken);

            var user = await _context.Users.FirstAsync(u => u.Id == id, cancellationToken);

            var changed = false;

            var deactivated = false;

            if (dto.FullName != null)
            {
                var fullName = dto.FullName.Trim();

                if (fullName.Length < 2 || fullName.Length > 100)
                {
                    throw new ArgumentException("نام و نام خانوادگی باید بین ۲ تا ۱۰۰ کاراکتر باشد.");
                }

                if (fullName != user.FullName)
                {
                    user.FullName = fullName;

                    changed = true;
                }
            }

            if (dto.IsActive.HasValue && dto.IsActive.Value != user.IsActive)
            {
                user.IsActive = dto.IsActive.Value;

                deactivated = !user.IsActive;

                changed = true;
            }

            if (changed)
            {
                user.UpdatedAt = DateTime.UtcNow;

                user.ConcurrencyStamp = Guid.NewGuid().ToString();

                await SaveAsync(cancellationToken);

                if (deactivated)
                {
                    var now = DateTime.UtcNow;

                    await _context.RefreshTokens
                        .Where(t => t.UserId == id && t.RevokedAt == null)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAt, now), cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

                _userAccessService.Invalidate(id);
            }

            return (await LoadUserAsync(actor, actorId, id, cancellationToken))!;
        }

        public async Task<UserDto> SetRolesAsync(int actorId, int id, EditUserRolesDto dto, CancellationToken cancellationToken = default)
        {
            var actor = await GetActorAsync(actorId, cancellationToken);

            if (id == actorId)
            {
                throw new ForbiddenException(SelfMessage);
            }

            var roleIds = dto.RoleIds.Distinct().ToList();

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            await LockUserAsync(id, cancellationToken);

            await EnsureCanManageAsync(actor, actorId, id, cancellationToken);

            var roles = await _context.Roles
                .AsNoTracking()
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => new { r.Id, r.Level, r.DisplayName })
                .ToListAsync(cancellationToken);

            if (roles.Count != roleIds.Count)
            {
                throw new ArgumentException("یک یا چند نقش انتخاب‌شده وجود ندارد.");
            }

            var forbidden = roles.FirstOrDefault(r => r.Level >= actor.Level);

            if (forbidden != null)
            {
                throw new ForbiddenException($"شما اجازه‌ی اختصاص نقش «{forbidden.DisplayName}» را ندارید.");
            }

            var current = await _context.UserRoles
                .Where(ur => ur.UserId == id)
                .Select(ur => ur.RoleId)
                .ToListAsync(cancellationToken);

            var toAdd = roleIds.Except(current).ToList();

            var toRemove = current.Except(roleIds).ToList();

            if (toRemove.Count > 0)
            {
                await _context.UserRoles
                    .Where(ur => ur.UserId == id && toRemove.Contains(ur.RoleId))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            if (toAdd.Count > 0)
            {
                _context.UserRoles.AddRange(toAdd.Select(roleId => new IdentityUserRole<int>
                {
                    UserId = id,
                    RoleId = roleId
                }));

                await SaveAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            _userAccessService.Invalidate(id);

            return (await LoadUserAsync(actor, actorId, id, cancellationToken))!;
        }
    }
}