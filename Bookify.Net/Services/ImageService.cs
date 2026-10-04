using SixLabors.ImageSharp.Processing;

namespace Bookify.Net.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private List<string> _allowedExtensions = new() { ".jpg", ".jpeg", ".png" };
        private int _maxAllowedSize = 2097152; // 2MB
                                               //**********************
        private const int ThumbWidth = 200;

        private static readonly Dictionary<string, byte[][]> Signatures = new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = [[0xFF, 0xD8, 0xFF]],
            [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
            [".png"] = [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]]
        };
        //**********************
        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<(bool isUploaded, string? errorMessage)> UploadAsync(IFormFile image, string imageName, string folderPath, bool hasThumbnail)
        {
            if (image is null || image.Length == 0)
                return (isUploaded: false, errorMessage: "Image Is Null Or Image Length = 0");

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!_allowedExtensions.Contains(extension))
                return (isUploaded: false, errorMessage: Errors.NotAllowedExtensions);

            if (image.Length > _maxAllowedSize)
                return (isUploaded: false, errorMessage: Errors.MaxSize);

            if (!await HasValidSignatureAsync(image, extension))
                return (isUploaded: false, errorMessage: Errors.NotAllowedExtensions);

            // حماية Path Traversal
            var folder = ResolveSafePath(folderPath);

            if (folder is null)
                return (isUploaded: false, errorMessage: Errors.NotAllowedExtensions);

            imageName = Path.GetFileName(imageName);

            // اسم الصورة يجب أن يحمل نفس امتداد الملف المرفوع (ImageSharp يختار الـ encoder منه)
            if (!string.Equals(Path.GetExtension(imageName), extension, StringComparison.OrdinalIgnoreCase))
                imageName = Path.ChangeExtension(imageName, extension);

            Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, imageName);
            string? thumbPath = null;

            try
            {
                // تحميل الصورة مرة واحدة فقط (يفشل إن لم تكن صورة حقيقية)
                await using var input = image.OpenReadStream();
                using var loadedImage = await Image.LoadAsync(input);

                loadedImage.Mutate(i => i.AutoOrient());
                loadedImage.Metadata.ExifProfile = null; // إزالة EXIF (GPS وغيره)

                await loadedImage.SaveAsync(path);

                if (hasThumbnail)
                {
                    var thumbFolder = Path.Combine(folder, "thumb");
                    Directory.CreateDirectory(thumbFolder);

                    thumbPath = Path.Combine(thumbFolder, imageName);

                    var ratio = (float)loadedImage.Width / ThumbWidth;
                    var height = loadedImage.Height / ratio;
                    loadedImage.Mutate(i => i.Resize(width: ThumbWidth, height: Math.Max(1, (int)height)));
                    await loadedImage.SaveAsync(thumbPath);
                }
            }
            catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException)
            {
                DeleteFiles(path, thumbPath);
                return (isUploaded: false, errorMessage: Errors.NotAllowedExtensions);
            }
            catch
            {
                // تنظيف أي ملف نصف مكتمل
                DeleteFiles(path, thumbPath);
                throw;
            }

            return (isUploaded: true, errorMessage: null);
        }
        public void Delete(string imagePath, string? imageThumbnailPath = null)
        {
            DeleteIfExists(imagePath);
            DeleteIfExists(imageThumbnailPath);
        }

        private void DeleteIfExists(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return;

            var fullPath = ResolveSafePath(relativePath);

            if (fullPath is null || !File.Exists(fullPath))
                return;

            try
            {
                File.Delete(fullPath);
            }
            catch (IOException)
            {
                // ملف مقفل: التنظيف محاولة فقط ولا نريد إيقاف الطلب
            }
        }
        private static async Task<bool> HasValidSignatureAsync(IFormFile image, string extension)
        {
            if (!Signatures.TryGetValue(extension, out var signatures))
                return false;

            var header = new byte[signatures.Max(s => s.Length)];

            await using var stream = image.OpenReadStream();
            var read = await stream.ReadAsync(header.AsMemory(0, header.Length));

            return signatures.Any(sig => read >= sig.Length && header.Take(sig.Length).SequenceEqual(sig));
        }
        private static void DeleteFiles(params string?[] paths)
        {
            foreach (var p in paths)
            {
                if (!string.IsNullOrEmpty(p) && File.Exists(p))
                    File.Delete(p);
            }
        }
        private string? ResolveSafePath(string relativePath)
        {
            var root = Path.GetFullPath(_webHostEnvironment.WebRootPath);
            var fullPath = Path.GetFullPath(Path.Combine(root, relativePath.TrimStart('/', '\\')));

            return fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                ? fullPath
                : null;
        }
    }
}