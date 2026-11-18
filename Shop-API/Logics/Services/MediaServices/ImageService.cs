using Microsoft.Extensions.Options;
using Shop_API.DTOs.MediaDTOs.ImageDTOs;
using Shop_API.Logics.IServices.IMediaServices;
using Shop_API.Options;
using SkiaSharp;

namespace Shop_API.Logics.Services.MediaServices
{
    public class ImageService : IImageService
    {
        private readonly string _rootPath;
        private readonly ImageStorageOptions _settings;
        private readonly SemaphoreSlim _processingSemaphore;

        public ImageService(IOptions<ImageStorageOptions> options)
        {
            _settings = options.Value;

            if (string.IsNullOrWhiteSpace(_settings.RootPath))
            {
                throw new InvalidOperationException("مسیر اصلی ذخیره‌سازی تصاویر تنظیم نشده است.");
            }

            if (!Path.IsPathFullyQualified(_settings.RootPath))
            {
                throw new InvalidOperationException("مسیر ذخیره‌سازی تصاویر باید یک مسیر کامل باشد.");
            }

            if (_settings.MaxUploadBytes <= 0 || _settings.MaxSourceSide <= 0 || _settings.MaxSourcePixels <= 0 || _settings.MainMaxSide <= 0 || _settings.ThumbnailMaxSide <= 0)
            {
                throw new InvalidOperationException("تنظیمات محدودیت حجم و ابعاد تصاویر باید بیشتر از صفر باشند.");
            }

            if (_settings.MainQuality is < 0 or > 100 || _settings.ThumbnailQuality is < 0 or > 100)
            {
                throw new InvalidOperationException("کیفیت تصاویر باید عددی بین صفر تا صد باشد.");
            }

            if (_settings.MaxConcurrentProcessing <= 0)
            {
                throw new InvalidOperationException("تعداد پردازش‌های هم‌زمان باید بیشتر از صفر باشد.");
            }

            _rootPath = Path.GetFullPath(_settings.RootPath);

            _processingSemaphore = new SemaphoreSlim(_settings.MaxConcurrentProcessing, _settings.MaxConcurrentProcessing);
        }

        private string GetStorageDirectory(string storageFolder)
        {
            if (string.IsNullOrWhiteSpace(storageFolder))
            {
                throw new ArgumentException("مسیر پوشه ذخیره‌سازی نمی‌تواند خالی باشد.", nameof(storageFolder));
            }

            var segments = storageFolder.Split(new[] { '/', '\\' }, StringSplitOptions.None);

            foreach (var segment in segments)
            {
                if (string.IsNullOrWhiteSpace(segment) || segment == "." || segment == ".." || !segment.All(c => char.IsAsciiLetterOrDigit(c) || c == '_' || c == '-'))
                {
                    throw new ArgumentException("مسیر پوشه ذخیره‌سازی معتبر نیست.", nameof(storageFolder));
                }
            }

            var fullPath = Path.GetFullPath(Path.Combine(_rootPath, Path.Combine(segments)));

            var rootWithSeparator = _rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            if (!fullPath.StartsWith(rootWithSeparator, comparison))
            {
                throw new UnauthorizedAccessException("دسترسی به مسیر خارج از محل مجاز ذخیره‌سازی امکان‌پذیر نیست.");
            }

            Directory.CreateDirectory(fullPath);

            return fullPath;
        }

        private void ValidateImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                throw new ArgumentException("لطفاً یک تصویر انتخاب کنید.");
            }

            if (image.Length > _settings.MaxUploadBytes)
            {
                var maxSizeInMb = _settings.MaxUploadBytes / (1024d * 1024d);

                throw new ArgumentException($"حجم تصویر نباید بیشتر از {maxSizeInMb:0.##} مگابایت باشد.");
            }

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("فرمت تصویر انتخاب‌شده مجاز نیست. فقط فرمت‌های JPG، JPEG، PNG و WebP پذیرفته می‌شوند.");
            }

            using var stream = image.OpenReadStream();
            using var codec = SKCodec.Create(stream);

            if (codec == null)
            {
                throw new ArgumentException("فایل انتخاب‌شده یک تصویر معتبر نیست یا قابل پردازش نیست.");
            }

            var isValidFormat = extension switch
            {
                ".jpg" or ".jpeg" => codec.EncodedFormat == SKEncodedImageFormat.Jpeg,
                ".png" => codec.EncodedFormat == SKEncodedImageFormat.Png,
                ".webp" => codec.EncodedFormat == SKEncodedImageFormat.Webp,
                _ => false
            };

            if (!isValidFormat)
            {
                throw new ArgumentException("فرمت واقعی تصویر با پسوند فایل مطابقت ندارد.");
            }

            var width = codec.Info.Width;
            var height = codec.Info.Height;

            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("ابعاد تصویر معتبر نیست.");
            }

            if (width > _settings.MaxSourceSide || height > _settings.MaxSourceSide)
            {
                throw new ArgumentException("ابعاد تصویر بیش از حد مجاز است.");
            }

            var totalPixels = (long)width * height;

            if (totalPixels > _settings.MaxSourcePixels)
            {
                throw new ArgumentException("تعداد پیکسل‌های تصویر بیش از حد مجاز است.");
            }
        }

        private (SKBitmap Bitmap, SKEncodedOrigin Origin) DecodeImage(Stream stream)
        {
            using var codec = SKCodec.Create(stream);

            if (codec == null)
            {
                throw new ArgumentException("خواندن تصویر امکان‌پذیر نیست یا فایل آسیب دیده است.");
            }

            var imageInfo = codec.Info;

            if (imageInfo.Width <= 0 || imageInfo.Height <= 0)
            {
                throw new ArgumentException("ابعاد تصویر معتبر نیست.");
            }

            var bitmap = new SKBitmap(imageInfo);

            if (bitmap.IsEmpty || bitmap.GetPixels() == IntPtr.Zero)
            {
                bitmap.Dispose();
                throw new InvalidOperationException("آماده‌سازی تصویر برای پردازش با خطا مواجه شد.");
            }

            var result = codec.GetPixels(imageInfo, bitmap.GetPixels());

            if (result != SKCodecResult.Success)
            {
                bitmap.Dispose();
                throw new ArgumentException("پردازش تصویر با خطا مواجه شد یا فایل تصویر ناقص است.");
            }

            return (bitmap, codec.EncodedOrigin);
        }

        private SKBitmap AutoOrient(SKBitmap bitmap, SKEncodedOrigin origin)
        {
            if (origin == SKEncodedOrigin.TopLeft)
            {
                return bitmap;
            }

            var width = bitmap.Width;
            var height = bitmap.Height;

            var swapsSides = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;

            var destinationWidth = swapsSides ? height : width;
            var destinationHeight = swapsSides ? width : height;

            var result = new SKBitmap(destinationWidth, destinationHeight, bitmap.ColorType, bitmap.AlphaType);

            if (result.IsEmpty)
            {
                result.Dispose();
                throw new InvalidOperationException("اصلاح جهت تصویر با خطا مواجه شد.");
            }

            try
            {
                var matrix = origin switch
                {
                    SKEncodedOrigin.TopRight => new SKMatrix
                    {
                        ScaleX = -1,
                        SkewX = 0,
                        TransX = width,
                        SkewY = 0,
                        ScaleY = 1,
                        TransY = 0,
                        Persp2 = 1
                    },

                    SKEncodedOrigin.BottomRight => new SKMatrix
                    {
                        ScaleX = -1,
                        SkewX = 0,
                        TransX = width,
                        SkewY = 0,
                        ScaleY = -1,
                        TransY = height,
                        Persp2 = 1
                    },

                    SKEncodedOrigin.BottomLeft => new SKMatrix
                    {
                        ScaleX = 1,
                        SkewX = 0,
                        TransX = 0,
                        SkewY = 0,
                        ScaleY = -1,
                        TransY = height,
                        Persp2 = 1
                    },

                    SKEncodedOrigin.LeftTop => new SKMatrix
                    {
                        ScaleX = 0,
                        SkewX = 1,
                        TransX = 0,
                        SkewY = 1,
                        ScaleY = 0,
                        TransY = 0,
                        Persp2 = 1
                    },

                    SKEncodedOrigin.RightTop => new SKMatrix
                    {
                        ScaleX = 0,
                        SkewX = -1,
                        TransX = height,
                        SkewY = 1,
                        ScaleY = 0,
                        TransY = 0,
                        Persp2 = 1
                    },

                    SKEncodedOrigin.RightBottom => new SKMatrix
                    {
                        ScaleX = 0,
                        SkewX = -1,
                        TransX = height,
                        SkewY = -1,
                        ScaleY = 0,
                        TransY = width,
                        Persp2 = 1
                    },

                    SKEncodedOrigin.LeftBottom => new SKMatrix
                    {
                        ScaleX = 0,
                        SkewX = 1,
                        TransX = 0,
                        SkewY = -1,
                        ScaleY = 0,
                        TransY = width,
                        Persp2 = 1
                    },

                    _ => throw new ArgumentException("جهت تصویر معتبر نیست.")
                };

                using var canvas = new SKCanvas(result);

                canvas.SetMatrix(matrix);

                canvas.DrawBitmap(bitmap, 0, 0, SKSamplingOptions.Default);

                canvas.Flush();

                bitmap.Dispose();

                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        private SKBitmap ResizeImage(SKBitmap source, int maxSide)
        {
            var longestSide = Math.Max(source.Width, source.Height);

            if (longestSide <= maxSide)
            {
                var copy = source.Copy();

                if (copy == null || copy.IsEmpty)
                {
                    copy?.Dispose();
                    throw new InvalidOperationException("کپی‌برداری از تصویر با خطا مواجه شد.");
                }

                return copy;
            }

            var scale = (float)maxSide / longestSide;

            var width = Math.Max(1,(int)Math.Round(source.Width * scale));

            var height = Math.Max(1, (int)Math.Round(source.Height * scale));

            var resized = new SKBitmap(width, height, source.ColorType, source.AlphaType);

            if (resized.IsEmpty)
            {
                resized.Dispose();
                throw new InvalidOperationException("تغییر ابعاد تصویر با خطا مواجه شد.");
            }

            try
            {
                using var canvas = new SKCanvas(resized);

                canvas.Clear(SKColors.Transparent);

                var sampling = new SKSamplingOptions(SKFilterMode.Linear);

                canvas.DrawBitmap(source, new SKRect(0, 0, width, height), sampling);

                canvas.Flush();

                return resized;
            }
            catch
            {
                resized.Dispose();
                throw;
            }
        }

        private byte[] EncodeWebp(SKBitmap bitmap, int quality)
        {
            using var image = SKImage.FromBitmap(bitmap);

            using var data = image.Encode(SKEncodedImageFormat.Webp, quality);

            if (data == null || data.Size == 0)
            {
                throw new InvalidOperationException("تبدیل تصویر به فرمت WebP با خطا مواجه شد.");
            }

            return data.ToArray();
        }

        private void DeleteTemporaryFile(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
            }
        }

        private void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private string GetStorageFilePath(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                throw new ArgumentException("مسیر فایل برای حذف معتبر نیست.", nameof(storageKey));
            }

            var normalizedKey = storageKey.Replace('\\', '/');

            var segments = normalizedKey.Split('/', StringSplitOptions.None);

            if (segments.Length < 2)
            {
                throw new ArgumentException("مسیر فایل برای حذف معتبر نیست.", nameof(storageKey));
            }

            for (var i = 0; i < segments.Length - 1; i++)
            {
                var segment = segments[i];

                if (string.IsNullOrWhiteSpace(segment) ||
                    segment == "." ||
                    segment == ".." ||
                    !segment.All(c =>
                        char.IsAsciiLetterOrDigit(c) ||
                        c == '_' ||
                        c == '-'))
                {
                    throw new ArgumentException("مسیر فایل برای حذف معتبر نیست.", nameof(storageKey));
                }
            }

            var fileName = segments[^1];

            if (!fileName.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("فرمت فایل برای حذف معتبر نیست.", nameof(storageKey));
            }

            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            if (!Guid.TryParseExact(nameWithoutExtension, "N", out _))
            {
                throw new ArgumentException("نام فایل برای حذف معتبر نیست.", nameof(storageKey));
            }

            var fullPath = Path.GetFullPath(Path.Combine(_rootPath, Path.Combine(segments)));

            var rootWithSeparator = _rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            if (!fullPath.StartsWith(rootWithSeparator, comparison))
            {
                throw new UnauthorizedAccessException("دسترسی به مسیر خارج از محل مجاز ذخیره‌سازی امکان‌پذیر نیست.");
            }

            return fullPath;
        }

        // Publics

        public async Task<ImageSaveResult> SaveAsync(IFormFile image, string storageFolder, CancellationToken cancellationToken = default)
        {
            await _processingSemaphore.WaitAsync(cancellationToken);

            SKBitmap? sourceBitmap = null;
            SKBitmap? orientedBitmap = null;

            string? mainPath = null;
            string? thumbnailPath = null;
            string? tempMainPath = null;
            string? tempThumbnailPath = null;

            try
            {
                ValidateImage(image);

                var directory = GetStorageDirectory(storageFolder);

                cancellationToken.ThrowIfCancellationRequested();

                using (var stream = image.OpenReadStream())
                {
                    var decoded = DecodeImage(stream);

                    sourceBitmap = decoded.Bitmap;

                    orientedBitmap = AutoOrient(sourceBitmap, decoded.Origin);

                    sourceBitmap = null;
                }

                cancellationToken.ThrowIfCancellationRequested();

                var source = orientedBitmap ?? throw new InvalidOperationException("خواندن تصویر با خطا مواجه شد.");

                using var mainBitmap = ResizeImage(source, _settings.MainMaxSide);

                using var thumbnailBitmap = ResizeImage(source, _settings.ThumbnailMaxSide);

                cancellationToken.ThrowIfCancellationRequested();

                var mainBytes = EncodeWebp(mainBitmap, _settings.MainQuality);

                var thumbnailBytes = EncodeWebp(thumbnailBitmap, _settings.ThumbnailQuality);

                cancellationToken.ThrowIfCancellationRequested();

                var fileName = Guid.NewGuid().ToString("N");

                mainPath = Path.Combine(directory, $"{fileName}.webp");

                thumbnailPath = Path.Combine(directory, $"{fileName}_thumb.webp");

                tempMainPath = mainPath + ".tmp";
                tempThumbnailPath = thumbnailPath + ".tmp";

                await File.WriteAllBytesAsync(tempMainPath, mainBytes, cancellationToken);

                await File.WriteAllBytesAsync(tempThumbnailPath, thumbnailBytes, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                File.Move(tempMainPath, mainPath);
                tempMainPath = null;

                File.Move(tempThumbnailPath, thumbnailPath);
                tempThumbnailPath = null;

                var storageKey = Path.Combine(storageFolder, $"{fileName}.webp").Replace('\\', '/');

                return new ImageSaveResult
                {
                    StorageKey = storageKey,
                    ContentType = "image/webp",
                    Size = mainBytes.LongLength,
                    Width = mainBitmap.Width,
                    Height = mainBitmap.Height
                };
            }
            catch
            {
                DeleteTemporaryFile(tempMainPath);
                DeleteTemporaryFile(tempThumbnailPath);
                DeleteTemporaryFile(mainPath);
                DeleteTemporaryFile(thumbnailPath);

                throw;
            }
            finally
            {
                sourceBitmap?.Dispose();
                orientedBitmap?.Dispose();

                _processingSemaphore.Release();
            }
        }

        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var mainPath = GetStorageFilePath(storageKey);

            var thumbnailPath = Path.Combine(Path.GetDirectoryName(mainPath)!, $"{Path.GetFileNameWithoutExtension(mainPath)}_thumb.webp");

            DeleteFile(mainPath);
            DeleteFile(thumbnailPath);

            return Task.CompletedTask;
        }

        public Task<(Stream Stream, string ContentType)?> OpenAsync(string storageKey, bool thumbnail = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var mainPath = GetStorageFilePath(storageKey);

            var filePath = thumbnail ? Path.Combine(Path.GetDirectoryName(mainPath)!, $"{Path.GetFileNameWithoutExtension(mainPath)}_thumb.webp") : mainPath;

            if (!File.Exists(filePath))
            {
                return Task.FromResult<(Stream Stream, string ContentType)?>(null);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 64 * 1024, options: FileOptions.Asynchronous | FileOptions.SequentialScan);

            return Task.FromResult<(Stream Stream, string ContentType)?>((stream, "image/webp"));
        }

    }
}
