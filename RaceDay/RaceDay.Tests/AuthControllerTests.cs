using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthControllerTests
    {
        private RaceDayDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        private IConfiguration GetTestConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", "ThisIsMySuperSecretKeyForRaceDayApi2026!@#$%^&*NeedAtLeast32Chars" },
                    { "Jwt:Issuer", "RaceDayAPI" },
                    { "Jwt:Audience", "RaceDayClient" }
                })
                .Build();
            return config;
        }

        [Fact]
        public async Task Register_WithNewEmail_Returns201()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var config = GetTestConfiguration();
            var controller = new AuthController(context, config);
            var dto = new RegisterDto
            {
                Email = "test@test.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                UserType = "Participant"
            };

            // Act
            var result = await controller.Register(dto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            context.Users.Count().Should().Be(1);
        }

        [Fact]
        public async Task Register_WithExistingEmail_Returns400()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var config = GetTestConfiguration();
            var controller = new AuthController(context, config);
            var dto = new RegisterDto
            {
                Email = "test@test.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                UserType = "Participant"
            };

            // Act
            await controller.Register(dto);
            var result = await controller.Register(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var config = GetTestConfiguration();
            var controller = new AuthController(context, config);

            await controller.Register(new RegisterDto
            {
                Email = "test@test.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                UserType = "Participant"
            });

            // Act
            var result = await controller.Login(new LoginDto
            {
                Email = "test@test.com",
                Password = "Password123!"
            });

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var config = GetTestConfiguration();
            var controller = new AuthController(context, config);

            await controller.Register(new RegisterDto
            {
                Email = "test@test.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                UserType = "Participant"
            });

            // Act
            var result = await controller.Login(new LoginDto
            {
                Email = "test@test.com",
                Password = "WrongPassword"
            });

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }
    }
}