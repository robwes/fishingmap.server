using AutoMapper;
using FishingMap.Common.Utils;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.AutoMapperProfiles;
using FishingMap.Domain.DTO.Users;
using FishingMap.Domain.Services;
using Moq;
using System.Linq.Expressions;

namespace FishingMap.Domain.Tests.Services.Tests
{
    public class UserServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private IMapper _mapper;
        private UserService _userService;

        public UserServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DomainProfile>();
            }).CreateMapper();

            _userService = new UserService(_unitOfWorkMock.Object, _mapper);
        }

        [Fact]
        public async Task AddUser_ShouldReturnUserDTO_WhenUserIsAddedSuccessfully()
        {
            // Arrange
            var userAdd = new UserAdd { UserName = "TestUser", Email = "test@example.com", Password = "TestPassword" };
            var userRole = new Role { Name = "User" };
            var addedUser = new User { UserName = userAdd.UserName, Email = userAdd.Email, Roles = new List<Role> { userRole } };

            _unitOfWorkMock.Setup(u => u.Users.Any(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(false);
            _unitOfWorkMock.Setup(u => u.Roles.GetAll(It.IsAny<Expression<Func<Role, bool>>>(), null, null, false)).ReturnsAsync(new List<Role> { userRole });
            _unitOfWorkMock.Setup(u => u.Users.Add(It.IsAny<User>())).Returns(addedUser);

            // Act
            var result = await _userService.AddUser(userAdd);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userAdd.UserName, result.UserName);
            Assert.Equal(userAdd.Email, result.Email);
            Assert.Contains(result.Roles, role => role.Name == "User");
            Assert.DoesNotContain(result.Roles, role => role.Name == "Administrator");
        }

        [Fact]
        public async Task AddUser_ShouldReturnNull_WhenUserAlreadyExists()
        {
            // Arrange
            var userAdd = new UserAdd { UserName = "TestUser", Email = "test@example.com", Password = "TestPassword" };

            _unitOfWorkMock.Setup(u => u.Users.Any(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(true);

            // Act
            var result = await _userService.AddUser(userAdd);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddUser_ShouldReturnNull_WhenPasswordIsEmpty()
        {
            // Arrange
            var userAdd = new UserAdd { UserName = "TestUser", Email = "test@example.com", Password = "" };

            // Act
            var result = await _userService.AddUser(userAdd);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAdministrator_ShouldReturnUserDTO_WhenUserIsAddedSuccessfully()
        {
            // Arrange
            var userAdd = new UserAdd { UserName = "TestAdmin", Email = "admin@example.com", Password = "AdminPassword" };
            var adminRole = new Role { Name = "Administrator" };
            var userRole = new Role { Name = "User" };
            var addedUser = new User { UserName = userAdd.UserName, Email = userAdd.Email, Roles = new List<Role> { adminRole, userRole } };

            _unitOfWorkMock.Setup(u => u.Users.Any(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(false);
            _unitOfWorkMock.Setup(u => u.Roles.GetAll(It.IsAny<Expression<Func<Role, bool>>>(), null, null, false)).ReturnsAsync(new List<Role> { adminRole, userRole });
            _unitOfWorkMock.Setup(u => u.Users.Add(It.IsAny<User>())).Returns(addedUser);

            // Act
            var result = await _userService.AddAdministrator(userAdd);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userAdd.UserName, result.UserName);
            Assert.Equal(userAdd.Email, result.Email);
            Assert.Contains(result.Roles, role => role.Name == "Administrator");
            Assert.Contains(result.Roles, role => role.Name == "User");
        }

        [Fact]
        public async Task AddAdministrator_ShouldReturnNull_WhenUserAlreadyExists()
        {
            // Arrange
            var userAdd = new UserAdd { UserName = "TestAdmin", Email = "admin@example.com", Password = "AdminPassword" };

            _unitOfWorkMock.Setup(u => u.Users.Any(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(true);

            // Act
            var result = await _userService.AddAdministrator(userAdd);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAdministrator_ShouldReturnNull_WhenPasswordIsEmpty()
        {
            // Arrange
            var userAdd = new UserAdd { UserName = "TestAdmin", Email = "admin@example.com", Password = "" };

            // Act
            var result = await _userService.AddAdministrator(userAdd);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteUser_ShouldCallDeleteAndSaveChanges_WhenCalledWithValidId()
        {
            // Arrange
            var userId = 1;

            _unitOfWorkMock.Setup(uow => uow.Users.Delete(userId)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            await _userService.DeleteUser(userId);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Users.Delete(userId), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task GetUser_ShouldReturnMappedUser_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var user = new User { 
                Id = userId,
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                UserName = "TestUser",
                Email = "TestEmail",
                Roles = new List<Role> { new Role { Name = "User" } }
            };

            _unitOfWorkMock.Setup(uow => uow.Users.GetUserWithRoles(userId, true)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUser(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.UserName, result.UserName);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.FirstName, result.FirstName);
            Assert.Equal(user.LastName, result.LastName);
            Assert.Single(result.Roles);
            Assert.Contains(result.Roles, role => role.Name == "User");
            _unitOfWorkMock.Verify(uow => uow.Users.GetUserWithRoles(userId, true), Times.Once);
        }

        [Fact]
        public async Task GetUser_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 1;

            _unitOfWorkMock.Setup(uow => uow.Users.GetUserWithRoles(userId, true)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUser(userId);

            // Assert
            Assert.Null(result);
        }
        [Fact]
        public async Task GetUserByEmail_ShouldReturnMappedUser_WhenUserExists()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User { Id = 1, Email = email };

            _unitOfWorkMock.Setup(uow => uow.Users.Find(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Expression<Func<User, object>>[]>(), It.IsAny<bool>())).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByEmail(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task GetUserByUsername_ShouldReturnMappedUser_WhenUserExists()
        {
            // Arrange
            var username = "testuser";
            var user = new User { Id = 1, UserName = username };

            _unitOfWorkMock.Setup(uow => uow.Users.Find(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Expression<Func<User, object>>[]>(), It.IsAny<bool>())).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByUsername(username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.UserName, result.UserName);
        }

        [Fact]
        public async Task GetUserCredentials_ShouldReturnMappedUserCredentials_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, UserName = "testuser", Password = "oldpassword", Salt = "oldsalt" };
            var userCredentials = new UserCredentials { UserName = "testuser", Password = "oldpassword", Salt = "oldsalt" };

            _unitOfWorkMock.Setup(uow => uow.Users.GetById(userId, null, true)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserCredentials(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userCredentials.UserName, result.UserName);
            Assert.Equal(userCredentials.Password, result.Password);
            Assert.Equal(userCredentials.Salt, result.Salt);
        }

        [Fact]
        public async Task UpdateUserDetails_ShouldUpdateUserAndReturnMappedUser_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var userDetailsUpdate = new UserDetailsUpdate { FirstName = "NewFirstName", LastName = "NewLastName", Email = "new@example.com" };
            var user = new User { Id = userId, FirstName = "OldFirstName", LastName = "OldLastName", Email = "old@example.com" };

            _unitOfWorkMock.Setup(uow => uow.Users.GetById(userId, null, false)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(uow => uow.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await _userService.UpdateUserDetails(userId, userDetailsUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userDetailsUpdate.FirstName, result.FirstName);
            Assert.Equal(userDetailsUpdate.LastName, result.LastName);
            Assert.Equal(userDetailsUpdate.Email, result.Email);
            _unitOfWorkMock.Verify(uow => uow.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task UpdateUserPassword_ShouldUpdatePasswordAndReturnTrue_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var password = "newpassword";
            var salt = "oldsalt";
            var hashedPassword = Cryptography.CreateHash(password, salt);
            var user = new User { Id = userId, Password = "oldpassword", Salt = salt };

            _unitOfWorkMock.Setup(uow => uow.Users.GetById(userId, null, false)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(uow => uow.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await _userService.UpdateUserPassword(userId, password);

            // Assert
            Assert.True(result);
            Assert.Equal(hashedPassword, user.Password); // Check that the password was hashed correctly
            _unitOfWorkMock.Verify(uow => uow.SaveChanges(), Times.Once);
        }
    }
}
