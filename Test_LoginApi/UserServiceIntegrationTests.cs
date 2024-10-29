using LoginApi.Context;
using LoginApi.Models;
using LoginApi.Repositories;
using LoginApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Test_LoginApi
{
    [TestClass]
    public class UserServiceIntegrationTests
    {
        private UserDbContext? _context;
        private IUserService? _userService;
        private IAsyncRepository<UserDbContext>? _repository;

        [TestInitialize]
        public void Setup()
        {
            // Set up in-memory database with a unique name to isolate test cases
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UserDbContext(options);

            // Inject repository and configuration dependencies
            _repository = new AsyncRepository<UserDbContext>(_context);
            var configurationMock = GetMockedConfiguration();

            _userService = new UserService(_repository, configurationMock.Object);

            // Debugging statement
            Console.WriteLine($"Token Key: {configurationMock.Object["AppSettings:Token"]}");
            Console.WriteLine($"UserService Initialized: {_userService != null}");
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

        // Helper method for creating and saving a user in the database
        private async Task AddUserToDatabaseAsync(string username, string password)
        {
            _context!.Users.Add(new User
            {
                UserName = username,
                Password = BCrypt.Net.BCrypt.HashPassword(password)
            });
            await _context.SaveChangesAsync();

            // Debugging statement
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            Console.WriteLine($"User Added: {user != null}");
        }

        [TestMethod]
        public async Task RegisterUser_ShouldStoreUserInDatabase_WhenCalled()
        {
            var userDto = new UserDto { UserName = "newuser", Password = "password123" };

            await _userService!.RegisterUser(userDto);

            var user = await _context!.Users.FirstOrDefaultAsync(u => u.UserName == "newuser");
            Assert.IsNotNull(user, "User should be added to the database");
            Assert.AreEqual("newuser", user.UserName, "Stored user should have the correct username");
        }

        [TestMethod]
        public async Task LoginUser_ShouldReturnToken_WhenCredentialsAreCorrect()
        {
            var userDto = new UserDto { UserName = "existinguser", Password = "password123" };

            // Add test user to the database
            await AddUserToDatabaseAsync("existinguser", "password123");

            var token = await _userService!.LoginUser(userDto);

            Console.WriteLine($"Generated Token in test method: {token}");

            Assert.IsNotNull(token, "Token should be generated for valid credentials");
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context!.Database.EnsureDeleted(); // Clean up in-memory database after each test
            _context.Dispose();
        }
    }
}


