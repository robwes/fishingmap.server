using FishingMap.Common.Utils;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.Interfaces;
using System.Security.Cryptography;

namespace FishingMap.Domain.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        public const int LifetimeDays = 30;

        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> IssueToken(int userId)
        {
            var rawToken = GenerateRawToken();
            AddTokenForUser(userId, rawToken);
            await _unitOfWork.SaveChanges();
            return rawToken;
        }

        public async Task<(int UserId, string NewToken)?> RotateToken(string rawToken)
        {
            var tokenHash = Cryptography.Sha256(rawToken);
            var token = await _unitOfWork.RefreshTokens.Find(t => t.TokenHash == tokenHash);
            if (token == null)
            {
                return null;
            }

            if (token.Revoked != null || token.Expires <= DateTime.UtcNow)
            {
                // A revoked token being presented again suggests it was stolen
                // (the legitimate client already rotated past it) — cut the whole chain.
                await RevokeAllTokensForUser(token.UserId);
                await _unitOfWork.SaveChanges();
                return null;
            }

            token.Revoked = DateTime.UtcNow;
            var newRawToken = GenerateRawToken();
            AddTokenForUser(token.UserId, newRawToken);
            await _unitOfWork.SaveChanges();

            return (token.UserId, newRawToken);
        }

        public async Task RevokeToken(string rawToken)
        {
            var tokenHash = Cryptography.Sha256(rawToken);
            var token = await _unitOfWork.RefreshTokens.Find(t => t.TokenHash == tokenHash);
            if (token != null && token.Revoked == null)
            {
                token.Revoked = DateTime.UtcNow;
                await _unitOfWork.SaveChanges();
            }
        }

        private void AddTokenForUser(int userId, string rawToken)
        {
            _unitOfWork.RefreshTokens.Add(new RefreshToken
            {
                UserId = userId,
                TokenHash = Cryptography.Sha256(rawToken),
                Expires = DateTime.UtcNow.AddDays(LifetimeDays),
                Created = DateTime.UtcNow
            });
        }

        private async Task RevokeAllTokensForUser(int userId)
        {
            var activeTokens = await _unitOfWork.RefreshTokens.GetAll(
                t => t.UserId == userId && t.Revoked == null);

            var now = DateTime.UtcNow;
            foreach (var activeToken in activeTokens)
            {
                activeToken.Revoked = now;
            }
        }

        private static string GenerateRawToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
