using LoginApi.Context;
using LoginApi.Models;
using LoginApi.Repositories;
using LoginApi.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Test_LoginApi
{
    [TestClass]
    public class UserServiceTests
    {
        private Mock<IAsyncRepository<UserDbContext>> _repositoryMock;
        private Mock<IConfiguration> _configurationMock;
        private IUserService _userService;

        [TestInitialize]
        public void Setup()
        {
            _repositoryMock = new Mock<IAsyncRepository<UserDbContext>>();
            _configurationMock = GetMockedConfiguration();

            _userService = new UserService(_repositoryMock.Object, _configurationMock.Object);
        }

        // Mock configuration for JWT token generation
        private Mock<IConfiguration> GetMockedConfiguration()
        {
            var configurationMock = new Mock<IConfiguration>();

            // Mocking the nested "AppSettings:Token" key correctly
            var sectionMock = new Mock<IConfigurationSection>();
            sectionMock.Setup(x => x.Value).Returns("supersecretkeythatshouldbeatleastsixtyfourcharacterslongandsecure!");

            configurationMock.Setup(x => x.GetSection("AppSettings:Token")).Returns(sectionMock.Object);

            return configurationMock;
        }

        [TestMethod]
        public async Task RegisterUser_ShouldRegisterUser_WhenUserDoesNotExist()
        {
            var userDto = new UserDto { UserName = "testuser", Password = "password123" };

            _repositoryMock.Setup(r => r.GetItem<User>(It.IsAny<Func<IQueryable<User>, IQueryable<User>>>()))
                .ReturnsAsync((User)null);

            await _userService.RegisterUser(userDto);

            _repositoryMock.Verify(r => r.AddItem(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        public async Task LoginUser_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var userDto = new UserDto { UserName = "testuser", Password = "password123" };
            var user = new User { UserName = "testuser", Password = BCrypt.Net.BCrypt.HashPassword("password123") };

            _repositoryMock.Setup(r => r.GetItem<User>(It.IsAny<Func<IQueryable<User>, IQueryable<User>>>()))
                .ReturnsAsync(user);

            var token = await _userService.LoginUser(userDto);

            Assert.IsNotNull(token);
        }

        [TestMethod]
        public async Task LoginUser_ShouldReturnNull_WhenCredentialsAreInvalid()
        {
            var userDto = new UserDto { UserName = "invalidUser", Password = "wrongpassword" };

            _repositoryMock.Setup(r => r.GetItem<User>(It.IsAny<Func<IQueryable<User>, IQueryable<User>>>()))
                .ReturnsAsync((User)null);

            var token = await _userService.LoginUser(userDto);

            Assert.IsNull(token);
        }
    }
}
