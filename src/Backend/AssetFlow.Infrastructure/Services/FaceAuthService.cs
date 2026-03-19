// ============================================================
// AssetFlow.Infrastructure / Services / FaceAuthService.cs
// Comparaison par distance cosinus entre keypoints
// ============================================================

using System.Text.Json;
using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using AssetFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Services
{
    public class FaceAuthService : IFaceAuthService
    {
        private readonly AppDbContext _dbContext;

        // Seuil de similarité : 0.98 = très strict, 0.95 = plus permissif
        // Ajustez selon vos tests (luminosité, angle, lunettes...)
        private const double SimilarityThreshold = 0.97;

        public FaceAuthService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LoginResponseDto?> FaceLoginAsync(FaceLoginRequestDto request)
        {
            // 1. Trouver l'utilisateur par email
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsApproved);

            if (user == null || string.IsNullOrEmpty(user.FaceKeypoints))
                return null;

            // 2. Désérialiser les keypoints stockés
            float[][]? stored;
            try
            {
                stored = JsonSerializer.Deserialize<float[][]>(user.FaceKeypoints);
            }
            catch
            {
                return null;
            }

            if (stored == null || stored.Length == 0)
                return null;

            // 3. Comparer par similarité cosinus
            var similarity = CosineSimilarity(request.Keypoints, stored);

            if (similarity < SimilarityThreshold)
                return null; // Visage non reconnu

            // 4. Retourner la réponse (sans token Keycloak pour simplifier)
            // Si vous voulez un vrai JWT, appelez Keycloak ici avec un grant spécial
            return new LoginResponseDto
            {
                UserId      = user.Id,
                AccessToken = GenerateFaceToken(user), // Token simplifié
                RefreshToken= string.Empty,
                ExpiresIn   = 3600,
                Role        = user.Role,
                FullName    = $"{user.FirstName} {user.LastName}"
            };
        }

        public async Task<bool> RegisterFaceAsync(RegisterFaceRequestDto request)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || request.Keypoints.Length == 0)
                return false;

            user.FaceKeypoints = JsonSerializer.Serialize(request.Keypoints);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────
        // Similarité cosinus sur les vecteurs aplatis
        // ────────────────────────────────────────────────
        private static double CosineSimilarity(float[][] a, float[][] b)
        {
            // Aplatir les tableaux 2D en vecteurs 1D
            var vecA = Flatten(a);
            var vecB = Flatten(b);

            int len = Math.Min(vecA.Length, vecB.Length);
            if (len == 0) return 0;

            double dot = 0, normA = 0, normB = 0;
            for (int i = 0; i < len; i++)
            {
                dot   += vecA[i] * vecB[i];
                normA += vecA[i] * vecA[i];
                normB += vecB[i] * vecB[i];
            }

            if (normA == 0 || normB == 0) return 0;
            return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        private static float[] Flatten(float[][] arr)
            => arr.SelectMany(p => p).ToArray();

        // Token simplifié (remplacez par Keycloak si besoin)
        private static string GenerateFaceToken(AssetFlow.Domain.Entities.User user)
        {
            var payload = new
            {
                sub      = user.Id.ToString(),
                email    = user.Email,
                role     = user.Role,
                fullName = $"{user.FirstName} {user.LastName}",
                exp      = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
            };
            var json = JsonSerializer.Serialize(payload);
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
        }
    }
}