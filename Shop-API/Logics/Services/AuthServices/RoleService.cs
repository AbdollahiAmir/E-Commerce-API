using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shop_API.Constants;
using Shop_API.Data;
using Shop_API.DTOs.RoleDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.IServices.IAuthServices;
using Shop_API.Models.AuthModels;

namespace Shop_API.Logics.Services.AuthServices
{
    public class RoleService : IRoleService
    {
        private const string InvalidAccessMessage = "دسترسی شما معتبر نیست.";
        private const string DuplicateNameMessage = "نقشی با این نام قبلاً ثبت شده است.";
        private const string SystemRoleMessage = "نقش‌های سیستمی قابل ویرایش و حذف نیستند.";
        private const string HigherRoleMessage = "شما اجازه‌ی تغییر نقشی با سطح برابر یا بالاتر از سطح خود را ندارید.";

        private static readonly Dictionary<string, string> PermissionNames = Permissions.All.ToDictionary(p => p.Key, p => p.DisplayName);

        private readonly AppDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUserAccessService _userAccessService;

        public RoleService(AppDbContext context, RoleManager<ApplicationRole> roleManager, IUserAccessService userAccessService)
        {
            _context = context;
            _roleManager = roleManager;
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

        private static string? NormalizeDescription(string? description)
        {
            var value = description?.Trim();

            return string.IsNullOrEmpty(value) ? null : value;
        }

        private static string NormalizeDisplayName(string displayName)
        {
            var value = displayName.Trim();

            if (value.Length < 2 || value.Length > 100)
            {
                throw new ArgumentException("عنوان نقش باید بین ۲ تا ۱۰۰ کاراکتر باشد.");
            }

            return value;
        }

        private static HashSet<string> NormalizePermissions(IEnumerable<string>? keys)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);

            if (keys == null)
            {
                return result;
            }

            foreach (var raw in keys)
            {
                var key = raw?.Trim();

                if (string.IsNullOrEmpty(key) || !PermissionNames.ContainsKey(key))
                {
                    throw new ArgumentException($"مجوز «{raw}» وجود ندارد.");
                }

                result.Add(key);
            }

            return result;
        }

        private static void EnsureActorHolds(UserAccess actor, IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                if (!actor.Permissions.Contains(key))
                {
                    throw new ForbiddenException($"شما مجوز «{PermissionNames[key]}» را ندارید و نمی‌توانید آن را به نقش اضافه یا از آن حذف کنید.");
                }
            }
        }

        private static void EnsureLevelAllowed(UserAccess actor, int level)
        {
            if (level < 1)
            {
                throw new ArgumentException("سطح نقش باید حداقل ۱ باشد.");
            }

            if (level >= actor.Level)
            {
                throw new ForbiddenException($"سطح نقش باید کمتر از سطح شما ({actor.Level}) باشد.");
            }
        }

        private static void EnsureManageable(UserAccess actor, ApplicationRole role)
        {
            if (role.IsSystem)
            {
                throw new ForbiddenException(SystemRoleMessage);
            }

            if (role.Level >= actor.Level)
            {
                throw new ForbiddenException(HigherRoleMessage);
            }
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("این نقش هم‌زمان توسط عملیات دیگری تغییر کرده است. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 2601 or 2627 })
            {
                throw new ConflictException(DuplicateNameMessage);
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 547 })
            {
                throw new ConflictException("این نقش به داده‌ی دیگری وابسته است و قابل حذف نیست.");
            }
        }

        private async Task LockRoleAsync(int id, CancellationToken cancellationToken)
        {
            var locked = await _context.Database
                .SqlQuery<int>($"SELECT [Id] AS [Value] FROM [Roles] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {id}")
                .ToListAsync(cancellationToken);

            if (locked.Count == 0)
            {
                throw new NotFoundException("نقش مورد نظر یافت نشد.");
            }
        }

        private async Task<RoleDto?> LoadRoleAsync(UserAccess actor, int id, CancellationToken cancellationToken)
        {
            var role = await _context.Roles
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name!,
                    DisplayName = r.DisplayName,
                    Description = r.Description,
                    Level = r.Level,
                    IsSystem = r.IsSystem,
                    CreatedAt = r.CreatedAt,
                    UserCount = _context.UserRoles.Count(ur => ur.RoleId == r.Id),
                    Permissions = _context.RoleClaims
                        .Where(c => c.RoleId == r.Id && c.ClaimType == Permissions.ClaimType)
                        .OrderBy(c => c.ClaimValue)
                        .Select(c => c.ClaimValue!)
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (role != null)
            {
                role.CanManage = !role.IsSystem && role.Level < actor.Level;
            }

            return role;
        }

        public async Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(int actorId, CancellationToken cancellationToken = default)
        {
            var actor = await GetActorAsync(actorId, cancellationToken);

            return Permissions.All.Select(p => new PermissionDto
            {
                Key = p.Key,
                DisplayName = p.DisplayName,
                Group = p.Group,
                CanGrant = actor.Permissions.Contains(p.Key)
            }).ToList();
        }

        public async Task<RoleDto> CreateAsync(int actorId, CreateRoleDto dto, CancellationToken cancellationToken = default)
        {
            var actor = await GetActorAsync(actorId, cancellationToken);

            EnsureLevelAllowed(actor, dto.Level);

            var permissions = NormalizePermissions(dto.Permissions);

            EnsureActorHolds(actor, permissions);

            var role = new ApplicationRole
            {
                Name = dto.Name.Trim(),
                DisplayName = NormalizeDisplayName(dto.DisplayName),
                Description = NormalizeDescription(dto.Description),
                Level = dto.Level,
                IsSystem = false
            };

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            IdentityResult result;

            try
            {
                result = await _roleManager.CreateAsync(role);

                if (result.Succeeded && permissions.Count > 0)
                {
                    _context.RoleClaims.AddRange(permissions.Select(key => new IdentityRoleClaim<int>
                    {
                        RoleId = role.Id,
                        ClaimType = Permissions.ClaimType,
                        ClaimValue = key
                    }));

                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 2601 or 2627 })
            {
                throw new ConflictException(DuplicateNameMessage);
            }

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateRoleName)))
                {
                    throw new ConflictException(DuplicateNameMessage);
                }

                throw new ArgumentException(string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            await transaction.CommitAsync(cancellationToken);

            return (await LoadRoleAsync(actor, role.Id, cancellationToken))!;
        }

        public async Task<IReadOnlyList<RoleListDto>> GetAllAsync(int actorId, CancellationToken cancellationToken = default)
        {
            var actor = await GetActorAsync(actorId, cancellationToken);

            var roles = await _context.Roles
                .AsNoTracking()
                .OrderByDescending(r => r.Level)
                .ThenBy(r => r.Name)
                .Select(r => new RoleListDto
                {
                    Id = r.Id,
                    Name = r.Name!,
                    DisplayName = r.DisplayName,
                    Description = r.Description,
                    Level = r.Level,
                    IsSystem = r.IsSystem,
                    UserCount = _context.UserRoles.Count(ur => ur.RoleId == r.Id),
                    PermissionCount = _context.RoleClaims.Count(c => c.RoleId == r.Id && c.ClaimType == Permissions.ClaimType)
                })
                .ToListAsync(cancellationToken);

            foreach (var role in roles)
            {
                role.CanManage = !role.IsSystem && role.Level < actor.Level;
            }

            return roles;
        }

        public async Task<RoleDto?> GetByIdAsync(int actorId, int id, CancellationToken cancellationToken = default)
        {
            var actor = await GetActorAsync(actorId, cancellationToken);

            return await LoadRoleAsync(actor, id, cancellationToken);
        }

        public async Task<RoleDto> EditAsync(int actorId, int id, EditRoleDto dto, CancellationToken cancellationToken = default)
        {
            var actor = await GetActorAsync(actorId, cancellationToken);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            await LockRoleAsync(id, cancellationToken);

            var role = await _context.Roles.FirstAsync(r => r.Id == id, cancellationToken);

            EnsureManageable(actor, role);

            var changed = false;

            if (dto.DisplayName != null)
            {
                var displayName = NormalizeDisplayName(dto.DisplayName);

                if (displayName != role.DisplayName)
                {
                    role.DisplayName = displayName;

                    changed = true;
                }
            }

            if (dto.Description != null)
            {
                var description = NormalizeDescription(dto.Description);

                if (description != role.Description)
                {
                    role.Description = description;

                    changed = true;
                }
            }

            if (dto.Level.HasValue && dto.Level.Value != role.Level)
            {
                EnsureLevelAllowed(actor, dto.Level.Value);

                role.Level = dto.Level.Value;

                changed = true;
            }

            if (dto.Permissions != null)
            {
                var desired = NormalizePermissions(dto.Permissions);

                var claims = await _context.RoleClaims
                    .Where(c => c.RoleId == id && c.ClaimType == Permissions.ClaimType)
                    .ToListAsync(cancellationToken);

                var existing = claims.Select(c => c.ClaimValue!).ToHashSet(StringComparer.Ordinal);

                var added = desired.Except(existing).ToList();

                var removed = existing.Except(desired).ToList();

                EnsureActorHolds(actor, added);

                EnsureActorHolds(actor, removed);

                if (removed.Count > 0)
                {
                    _context.RoleClaims.RemoveRange(claims.Where(c => removed.Contains(c.ClaimValue!)));
                }

                if (added.Count > 0)
                {
                    _context.RoleClaims.AddRange(added.Select(key => new IdentityRoleClaim<int>
                    {
                        RoleId = id,
                        ClaimType = Permissions.ClaimType,
                        ClaimValue = key
                    }));
                }

                if (added.Count > 0 || removed.Count > 0)
                {
                    changed = true;
                }
            }

            if (changed)
            {
                role.ConcurrencyStamp = Guid.NewGuid().ToString();

                await SaveAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                _userAccessService.InvalidateAll();
            }

            return (await LoadRoleAsync(actor, id, cancellationToken))!;
        }

        public async Task DeleteAsync(int actorId, int id, CancellationToken cancellationToken = default)
        {
            var actor = await GetActorAsync(actorId, cancellationToken);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            await LockRoleAsync(id, cancellationToken);

            var role = await _context.Roles.FirstAsync(r => r.Id == id, cancellationToken);

            EnsureManageable(actor, role);

            var userCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == id, cancellationToken);

            if (userCount > 0)
            {
                throw new ConflictException($"این نقش به {userCount} کاربر اختصاص داده شده و قابل حذف نیست.");
            }

            _context.Roles.Remove(role);

            await SaveAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
    }
}