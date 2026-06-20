using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using ShopNest.Application.Interfaces;
using ShopNest.Infrastructure.Settings;
using AppImageUploadResult = ShopNest.Application.Dtos.ImageUploadResult;

namespace ShopNest.Infrastructure.Services;

public sealed class CloudinaryImageStorageService(IOptions<CloudinarySettings> options) : IImageStorageService
{
    public async Task<AppImageUploadResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.CloudName))
        {
            var uniqueName = $"{Guid.NewGuid()}-{fileName}";
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var filePath = Path.Combine(folder, uniqueName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream, cancellationToken);
            }
            return new AppImageUploadResult($"/uploads/{uniqueName}", string.Empty);
        }

        var cloudinary = new Cloudinary(new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret));
        var result = await cloudinary.UploadAsync(new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = "shopnest/products"
        }, cancellationToken);

        return new AppImageUploadResult(result.SecureUrl.ToString(), result.PublicId);
    }
}
