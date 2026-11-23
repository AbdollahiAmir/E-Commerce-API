using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shop_API.Data;
using Shop_API.DTOs.MediaDTOs;
using Shop_API.DTOs.VariantDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.IServices.IProductServices;
using Shop_API.Models.ProductModels;

namespace Shop_API.Logics.Services.ProductServices
{
    public class VariantService : IVariantService
    {
        private const int MaxPageSize = 100;
        private const int SkuMaxLength = 100;
        private const int ValueMaxLength = 150;
        private const int MaxAttributesPerVariant = 10;
        private const decimal MaxPrice = 1_000_000_000_000m;
        private const string ReferenceDeletedMessage = "محصول یا ویژگی انتخاب‌شده هم‌زمان حذف شد. لطفاً دوباره تلاش کنید.";
        private const string DeleteBlockedMessage = "این نوع محصول به داده‌ی دیگری وابسته است و قابل حذف نیست.";

        private sealed record ResolvedAttribute(int AttributeId, string Value, string? ColorCode);

        private readonly AppDbContext _context;

        public VariantService(AppDbContext context)
        {
            _context = context;
        }

        private static string? NormalizeDescription(string? description)
        {
            var value = description?.Trim();

            return string.IsNullOrEmpty(value) ? null : value;
        }

        private static string NormalizeSku(string? sku)
        {
            var value = (sku ?? string.Empty).Trim().ToUpperInvariant();

            if (value.Length == 0 || value.Length > SkuMaxLength || !char.IsAsciiLetterOrDigit(value[0]) || !value.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-'))
            {
                throw new ArgumentException("SKU فقط می‌تواند شامل حروف انگلیسی، عدد و نشانه‌های . _ - باشد و باید با حرف یا عدد شروع شود.");
            }

            return value;
        }

        private static decimal ValidatePrice(decimal price)
        {
            if (price < 0 || price > MaxPrice)
            {
                throw new ArgumentException("قیمت باید بین صفر و یک تریلیون باشد.");
            }

            if (decimal.Round(price, 2) != price)
            {
                throw new ArgumentException("قیمت حداکثر می‌تواند دو رقم اعشار داشته باشد.");
            }

            return price;
        }

        private static int ValidateStock(int stock)
        {
            if (stock < 0)
            {
                throw new ArgumentException("موجودی نمی‌تواند منفی باشد.");
            }

            return stock;
        }

        private static bool IsValidHexColor(string color)
        {
            return color.Length == 7 && color[0] == '#' && color.Skip(1).All(char.IsAsciiHexDigit);
        }

        private static string BuildKey(IEnumerable<(int AttributeId, string Value)> items)
        {
            return string.Join("|", items.OrderBy(i => i.AttributeId).Select(i => $"{i.AttributeId}:{i.Value.ToUpperInvariant()}"));
        }

        private async Task<List<ResolvedAttribute>> ResolveAttributesAsync(List<VariantAttributeInputDto> inputs, CancellationToken cancellationToken)
        {
            if (inputs.Count > MaxAttributesPerVariant)
            {
                throw new ArgumentException($"هر نوع محصول حداکثر می‌تواند {MaxAttributesPerVariant} ویژگی داشته باشد.");
            }

            var ids = inputs.Select(i => i.AttributeId).ToList();

            if (ids.Distinct().Count() != ids.Count)
            {
                throw new ArgumentException("یک ویژگی نمی‌تواند بیش از یک‌بار برای یک نوع محصول ثبت شود.");
            }

            var attributes = await _context.ProductAttributes.AsNoTracking().Where(a => ids.Contains(a.Id)).ToDictionaryAsync(a => a.Id, cancellationToken);

            var result = new List<ResolvedAttribute>();

            foreach (var input in inputs)
            {
                if (!attributes.TryGetValue(input.AttributeId, out var attribute))
                {
                    throw new ArgumentException("یک یا چند ویژگی انتخاب‌شده یافت نشد.");
                }

                var value = input.Value?.Trim() ?? string.Empty;

                if (value.Length == 0)
                {
                    throw new ArgumentException($"مقدار ویژگی «{attribute.Name}» نمی‌تواند خالی باشد.");
                }

                if (value.Length > ValueMaxLength)
                {
                    throw new ArgumentException($"مقدار ویژگی «{attribute.Name}» نباید بیشتر از {ValueMaxLength} کاراکتر باشد.");
                }

                var colorCode = string.IsNullOrWhiteSpace(input.ColorCode) ? null : input.ColorCode.Trim().ToUpperInvariant();

                if (attribute.IsColor && colorCode == null)
                {
                    throw new ArgumentException($"برای ویژگی رنگی «{attribute.Name}» کد رنگ الزامی است.");
                }

                if (!attribute.IsColor && colorCode != null)
                {
                    throw new ArgumentException($"کد رنگ فقط برای ویژگی‌های رنگی مجاز است و «{attribute.Name}» رنگی نیست.");
                }

                if (colorCode != null && !IsValidHexColor(colorCode))
                {
                    throw new ArgumentException("کد رنگ باید به شکل #RRGGBB باشد (مثلاً #FF0000).");
                }

                result.Add(new ResolvedAttribute(attribute.Id, value, colorCode));
            }

            return result;
        }

        private async Task EnsureCombinationUniqueAsync(int productId, int? excludeVariantId, List<ResolvedAttribute> attributes, CancellationToken cancellationToken)
        {
            var others = await _context.ProductVariants.AsNoTracking().Where(v => v.ProductId == productId && (excludeVariantId == null || v.Id != excludeVariantId)).Select(v => new
                {
                    Values = v.AttributeValues.Select(a => new { a.ProductAttributeId, a.Value }).ToList()
                }).ToListAsync(cancellationToken);

            var key = BuildKey(attributes.Select(a => (a.AttributeId, a.Value)));

            foreach (var other in others)
            {
                if (BuildKey(other.Values.Select(a => (a.ProductAttributeId, a.Value))) == key)
                {
                    throw new ConflictException("برای این محصول نوعی با همین ترکیب ویژگی‌ها وجود دارد.");
                }
            }
        }

        private async Task SaveAsync(string foreignKeyConflictMessage, CancellationToken cancellationToken)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("این نوع محصول هم‌زمان توسط عملیات دیگری تغییر کرده است. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 2601 or 2627 })
            {
                throw new ConflictException("این SKU هم‌زمان توسط نوع دیگری ثبت شد. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 547 })
            {
                throw new ConflictException(foreignKeyConflictMessage);
            }
        }

        private async Task<VariantDto> GetRequiredDtoAsync(int id, CancellationToken cancellationToken)
        {
            return await GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("نوع محصول مورد نظر یافت نشد.");
        }

        public async Task<VariantDto> CreateAsync(CreateVariantDto dto, CancellationToken cancellationToken = default)
        {
            var productExists = await _context.Products.AnyAsync(p => p.Id == dto.ProductId, cancellationToken);

            if (!productExists)
            {
                throw new ArgumentException("محصول انتخاب‌شده یافت نشد.");
            }

            var sku = NormalizeSku(dto.Sku);

            var skuExists = await _context.ProductVariants.AnyAsync(v => v.SKU == sku, cancellationToken);

            if (skuExists)
            {
                throw new ConflictException("این SKU قبلاً ثبت شده است.");
            }

            var price = ValidatePrice(dto.Price ?? throw new ArgumentException("قیمت الزامی است."));

            var stock = ValidateStock(dto.StockQuantity);

            var attributes = await ResolveAttributesAsync(dto.Attributes ?? [], cancellationToken);

            await EnsureCombinationUniqueAsync(dto.ProductId, null, attributes, cancellationToken);

            var variant = new ProductVariant
            {
                ProductId = dto.ProductId,
                SKU = sku,
                Description = NormalizeDescription(dto.Description),
                Price = price,
                StockQuantity = stock,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var item in attributes)
            {
                variant.AttributeValues.Add(new VariantAttribute
                {
                    ProductAttributeId = item.AttributeId,
                    Value = item.Value,
                    ColorCode = item.ColorCode
                });
            }

            _context.ProductVariants.Add(variant);

            await SaveAsync(ReferenceDeletedMessage, cancellationToken);

            return await GetRequiredDtoAsync(variant.Id, cancellationToken);
        }

        public async Task<PagedResult<VariantListDto>> GetPagedAsync(int page, int pageSize, string? search, int? productId, bool? isActive, CancellationToken cancellationToken = default)
        {
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

            var query = _context.ProductVariants.AsNoTracking();

            var term = search?.Trim();

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(v => v.SKU.Contains(term));
            }

            if (productId.HasValue)
            {
                query = query.Where(v => v.ProductId == productId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(v => v.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query.OrderByDescending(v => v.CreatedAt).ThenByDescending(v => v.Id).Skip((int)skip).Take(pageSize).Select(v => new VariantListDto
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    ProductName = v.Product.Name,
                    Sku = v.SKU,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    IsActive = v.IsActive
                }).ToListAsync(cancellationToken);

            return new PagedResult<VariantListDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public Task<VariantDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.ProductVariants.AsNoTracking().Where(v => v.Id == id).Select(v => new VariantDto
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    ProductName = v.Product.Name,
                    Sku = v.SKU,
                    Description = v.Description,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    IsActive = v.IsActive,
                    Attributes = v.AttributeValues.OrderBy(a => a.ProductAttribute.Name).Select(a => new VariantAttributeValueDto
                        {
                            AttributeId = a.ProductAttributeId,
                            AttributeName = a.ProductAttribute.Name,
                            AttributeCode = a.ProductAttribute.Code,
                            IsColor = a.ProductAttribute.IsColor,
                            Value = a.Value,
                            ColorCode = a.ColorCode
                        }).ToList(),
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                }).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<VariantDto> EditAsync(int id, EditVariantDto dto, CancellationToken cancellationToken = default)
        {
            var variant = await _context.ProductVariants.Include(v => v.AttributeValues).FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
                ?? throw new NotFoundException("نوع محصول مورد نظر یافت نشد.");

            var changed = false;

            if (dto.Sku != null)
            {
                var sku = NormalizeSku(dto.Sku);

                if (sku != variant.SKU)
                {
                    var skuExists = await _context.ProductVariants.AnyAsync(v => v.SKU == sku && v.Id != variant.Id, cancellationToken);

                    if (skuExists)
                    {
                        throw new ConflictException("این SKU قبلاً برای نوع دیگری ثبت شده است.");
                    }

                    variant.SKU = sku;

                    changed = true;
                }
            }

            if (dto.Description != null)
            {
                var description = NormalizeDescription(dto.Description);

                if (description != variant.Description)
                {
                    variant.Description = description;

                    changed = true;
                }
            }

            if (dto.Price.HasValue)
            {
                var price = ValidatePrice(dto.Price.Value);

                if (price != variant.Price)
                {
                    variant.Price = price;

                    changed = true;
                }
            }

            if (dto.StockQuantity.HasValue)
            {
                var stock = ValidateStock(dto.StockQuantity.Value);

                if (stock != variant.StockQuantity)
                {
                    variant.StockQuantity = stock;

                    changed = true;
                }
            }

            if (dto.IsActive.HasValue && dto.IsActive.Value != variant.IsActive)
            {
                variant.IsActive = dto.IsActive.Value;

                changed = true;
            }

            if (dto.Attributes != null)
            {
                var attributes = await ResolveAttributesAsync(dto.Attributes, cancellationToken);

                await EnsureCombinationUniqueAsync(variant.ProductId, variant.Id, attributes, cancellationToken);

                var desiredIds = attributes.Select(a => a.AttributeId).ToHashSet();

                foreach (var removed in variant.AttributeValues.Where(v => !desiredIds.Contains(v.ProductAttributeId)).ToList())
                {
                    _context.VariantAttributes.Remove(removed);

                    changed = true;
                }

                foreach (var item in attributes)
                {
                    var existing = variant.AttributeValues.FirstOrDefault(v => v.ProductAttributeId == item.AttributeId);

                    if (existing == null)
                    {
                        variant.AttributeValues.Add(new VariantAttribute
                        {
                            ProductAttributeId = item.AttributeId,
                            Value = item.Value,
                            ColorCode = item.ColorCode
                        });

                        changed = true;
                    }
                    else if (existing.Value != item.Value || existing.ColorCode != item.ColorCode)
                    {
                        existing.Value = item.Value;
                        existing.ColorCode = item.ColorCode;

                        changed = true;
                    }
                }
            }

            if (changed)
            {
                variant.UpdatedAt = DateTime.UtcNow;

                await SaveAsync(ReferenceDeletedMessage, cancellationToken);
            }

            return await GetRequiredDtoAsync(variant.Id, cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == id, cancellationToken) ?? throw new NotFoundException("نوع محصول مورد نظر یافت نشد.");

            _context.ProductVariants.Remove(variant);

            await SaveAsync(DeleteBlockedMessage, cancellationToken);
        }
    }
}
