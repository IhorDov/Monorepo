using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LoginApi.Controllers;
using LoginApi.Services;
using LoginApi.Models;


namespace Test_LoginApi
{
    [TestClass]
    public class UserAuthControllerTests
    {
        private Mock<IUserService> _userServiceMock;
        private UserAuthController? _controller;

        [TestInitialize]
        public void Setup()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new UserAuthController((IUserService)_userServiceMock.Object);
        }

        // Register Tests
        [TestMethod]
        public async Task Register_ShouldReturnOk_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var userDto = new UserDto { UserName = "testuser", Password = "password" };
            _userServiceMock.Setup(s => s.RegisterUser(userDto)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(userDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual("User registered successfully", (result as OkObjectResult)?.Value);
        }

        [TestMethod]
        public async Task Register_ShouldReturnBadRequest_WhenRegistrationFails()
        {
            // Arrange
            var userDto = new UserDto { UserName = "testuser", Password = "password" };
            _userServiceMock.Setup(s => s.RegisterUser(userDto)).Throws(new System.Exception("Registration error"));

            // Act
            var result = await _controller.Register(userDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual("Registration error", (result as BadRequestObjectResult)?.Value);
        }

        // Login Tests
        [TestMethod]
        public async Task Login_ShouldReturnOkWithToken_WhenLoginIsSuccessful()
        {
            // Arrange
            var userDto = new UserDto { UserName = "testuser", Password = "password" };
            var token = "sample_jwt_token";
            _userServiceMock.Setup(s => s.LoginUser(userDto)).ReturnsAsync(token);

            // Act
            var result = await _controller.Login(userDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            // Cast result to OkObjectResult
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            // Access the token from the anonymous type directly
            var tokenValue = okResult.Value?.GetType().GetProperty("Token")?.GetValue(okResult.Value, null);
            Assert.IsNotNull(tokenValue);
            Assert.AreEqual(token, tokenValue);
        }

        [TestMethod]
        public async Task Login_ShouldReturnUnauthorized_WhenLoginFails()
        {
            // Arrange
            var userDto = new UserDto { UserName = "testuser", Password = "wrongpassword" };
            _userServiceMock.Setup(s => s.LoginUser(userDto)).ReturnsAsync((string)null);

            // Act
            var result = await _controller.Login(userDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            Assert.AreEqual("Invalid username or password", (result as UnauthorizedObjectResult)?.Value);
        }

    }
}