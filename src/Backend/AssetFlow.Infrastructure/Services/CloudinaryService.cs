using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace AssetFlow.Infrastructure.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        // Upload base64 → retourne l'URL publique
        public async Task<string?> UploadBase64Async(string base64Data, string publicId)
        {
            // base64Data = "data:image/png;base64,iVBOR..."
            var uploadParams = new ImageUploadParams
            {
                File       = new FileDescription(publicId, new MemoryStream(
                    Convert.FromBase64String(base64Data.Split(',').Last()))),
                PublicId   = $"materiels/{publicId}",
                Overwrite  = true,
                Folder     = "assetflow/materiels"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.Error == null ? result.SecureUrl.ToString() : null;
        }

        // Supprimer une image par son URL publique Cloudinary
        public async Task<bool> DeleteByUrlAsync(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return true;
            if (!imageUrl.Contains("cloudinary.com")) return true; // pas une image Cloudinary

            // Extraire le public_id depuis l'URL
            // Ex: https://res.cloudinary.com/cloud/image/upload/v123/assetflow/materiels/abc.png
            var uri      = new Uri(imageUrl);
            var segments = uri.AbsolutePath.Split('/');
            // Trouver l'index après "upload"
            var uploadIdx = Array.IndexOf(segments, "upload");
            if (uploadIdx < 0) return false;

            // Ignorer le segment version (v123456)
            var pathSegments = segments.Skip(uploadIdx + 1)
                                       .SkipWhile(s => s.StartsWith("v") && s.Length > 1 && s.Skip(1).All(char.IsDigit))
                                       .ToArray();

            var publicId = string.Join("/", pathSegments);
            // Enlever l'extension
            publicId = publicId.Contains('.') ? publicId[..publicId.LastIndexOf('.')] : publicId;

            var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
            return result.Result == "ok";
        }
    }
}