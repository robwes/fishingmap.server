namespace FishingMap.Domain.Interfaces
{
    public interface IRefreshTokenService
    {
        /// <summary>Creates and persists a new refresh token; returns the raw value for the cookie.</summary>
        Task<string> IssueToken(int userId);

        /// <summary>
        /// Validates and rotates a refresh token. Returns the user id and the replacement
        /// raw token, or null if the token is unknown, expired, or revoked (reuse of a
        /// revoked token additionally revokes all of that user's tokens).
        /// </summary>
        Task<(int UserId, string NewToken)?> RotateToken(string rawToken);

        /// <summary>Revokes a refresh token if it is still active (used on logout).</summary>
        Task RevokeToken(string rawToken);
    }
}
