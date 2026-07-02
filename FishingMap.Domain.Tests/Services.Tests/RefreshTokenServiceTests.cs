using FishingMap.Common.Utils;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.Services;
using Moq;
using System.Linq.Expressions;

namespace FishingMap.Domain.Tests.Services.Tests
{
    public class RefreshTokenServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRefreshTokenRepository> _refreshTokensRepoMock;
        private readonly RefreshTokenService _service;

        public RefreshTokenServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _refreshTokensRepoMock = new Mock<IRefreshTokenRepository>();
            _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokensRepoMock.Object);

            _service = new RefreshTokenService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task IssueToken_PersistsHashOfReturnedToken()
        {
            RefreshToken? added = null;
            _refreshTokensRepoMock.Setup(r => r.Add(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(t => added = t)
                .Returns<RefreshToken>(t => t);

            var rawToken = await _service.IssueToken(42);

            Assert.NotNull(added);
            Assert.Equal(42, added.UserId);
            Assert.NotEqual(rawToken, added.TokenHash);
            Assert.Equal(Cryptography.Sha256(rawToken), added.TokenHash);
            Assert.True(added.Expires > DateTime.UtcNow.AddDays(RefreshTokenService.LifetimeDays - 1));
            Assert.Null(added.Revoked);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task RotateToken_ReturnsNull_WhenTokenUnknown()
        {
            _refreshTokensRepoMock.Setup(r => r.Find(It.IsAny<Expression<Func<RefreshToken, bool>>>(), null, false))
                .ReturnsAsync((RefreshToken?)null);

            var result = await _service.RotateToken("unknown-token");

            Assert.Null(result);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Never);
        }

        [Fact]
        public async Task RotateToken_RevokesOldAndIssuesNew_WhenTokenValid()
        {
            var existing = new RefreshToken
            {
                Id = 1,
                UserId = 42,
                TokenHash = "hash",
                Expires = DateTime.UtcNow.AddDays(10),
                Created = DateTime.UtcNow.AddDays(-20)
            };
            _refreshTokensRepoMock.Setup(r => r.Find(It.IsAny<Expression<Func<RefreshToken, bool>>>(), null, false))
                .ReturnsAsync(existing);

            RefreshToken? added = null;
            _refreshTokensRepoMock.Setup(r => r.Add(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(t => added = t)
                .Returns<RefreshToken>(t => t);

            var result = await _service.RotateToken("raw-token");

            Assert.NotNull(result);
            Assert.Equal(42, result.Value.UserId);
            Assert.NotNull(existing.Revoked);
            Assert.NotNull(added);
            Assert.Equal(Cryptography.Sha256(result.Value.NewToken), added.TokenHash);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task RotateToken_RevokesAllUserTokens_WhenTokenAlreadyRevoked()
        {
            var revoked = new RefreshToken
            {
                Id = 1,
                UserId = 42,
                TokenHash = "hash",
                Expires = DateTime.UtcNow.AddDays(10),
                Revoked = DateTime.UtcNow.AddMinutes(-5)
            };
            var otherActive = new RefreshToken
            {
                Id = 2,
                UserId = 42,
                TokenHash = "hash2",
                Expires = DateTime.UtcNow.AddDays(10)
            };
            _refreshTokensRepoMock.Setup(r => r.Find(It.IsAny<Expression<Func<RefreshToken, bool>>>(), null, false))
                .ReturnsAsync(revoked);
            _refreshTokensRepoMock.Setup(r => r.GetAll(It.IsAny<Expression<Func<RefreshToken, bool>>>(), null, null, false))
                .ReturnsAsync(new List<RefreshToken> { otherActive });

            var result = await _service.RotateToken("stolen-token");

            Assert.Null(result);
            Assert.NotNull(otherActive.Revoked);
            _refreshTokensRepoMock.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task RotateToken_ReturnsNull_WhenTokenExpired()
        {
            var expired = new RefreshToken
            {
                Id = 1,
                UserId = 42,
                TokenHash = "hash",
                Expires = DateTime.UtcNow.AddDays(-1)
            };
            _refreshTokensRepoMock.Setup(r => r.Find(It.IsAny<Expression<Func<RefreshToken, bool>>>(), null, false))
                .ReturnsAsync(expired);
            _refreshTokensRepoMock.Setup(r => r.GetAll(It.IsAny<Expression<Func<RefreshToken, bool>>>(), null, null, false))
                .ReturnsAsync(new List<RefreshToken>());

            var result = await _service.RotateToken("old-token");

            Assert.Null(result);
            _refreshTokensRepoMock.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task RevokeToken_SetsRevoked_WhenTokenActive()
        {
            var active = new RefreshToken
            {
                Id = 1,
                UserId = 42,
                TokenHash = "hash",
                Expires = DateTime.UtcNow.AddDays(10)
            };
            _refreshTokensRepoMock.Setup(r => r.Find(It.IsAny<Expression<Func<RefreshToken, bool>>>(), null, false))
                .ReturnsAsync(active);

            await _service.RevokeToken("raw-token");

            Assert.NotNull(active.Revoked);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
