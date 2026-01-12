using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using LibraryAPI.Services;
using LibraryAPI.Repositories;
using LibraryAPI.DTOs;
using LibraryAPI.Models;
using System.Threading.Tasks;

namespace LibraryAPI.Tests.Services
{
    /// <summary>
    /// Unit tests for AuthService
    /// Interview Note: Demonstrates testing authentication logic, password hashing, and JWT generation
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Setup JWT configuration mocks
            var jwtSettingsSection = new Mock<IConfigurationSection>();
            jwtSettingsSection.Setup(x => x["SecretKey"]).Returns("YourSuperSecretKeyForJWTTokenGeneration2024!MustBeAtLeast32Characters");
            jwtSettingsSection.Setup(x => x["Issuer"]).Returns("LibraryAPI");
            jwtSettingsSection.Setup(x => x["Audience"]).Returns("LibraryAPIUsers");

            _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSettingsSection.Object);

            _authService = new AuthService(_mockUserRepository.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsAuthResponse_WhenUserDoesNotExist()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "SecurePass123",
                ConfirmPassword = "SecurePass123"
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync((User?)null);

            _mockUserRepository.Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => { u.Id = 1; return u; });

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("john@example.com", result.Email);
            Assert.Equal("John Doe", result.FullName);
            Assert.NotEmpty(result.Token);
            Assert.True(result.ExpiresAt > DateTime.UtcNow);

            _mockUserRepository.Verify(repo => repo.GetByEmailAsync(registerDto.Email), Times.Once);
            _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsNull_WhenUserAlreadyExists()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "existing@example.com",
                Password = "SecurePass123"
            };

            var existingUser = new User { Id = 1, Email = "existing@example.com" };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            Assert.Null(result);
            _mockUserRepository.Verify(repo => repo.GetByEmailAsync(registerDto.Email), Times.Once);
            _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UserExistsAsync_ReturnsTrue_WhenUserExists()
        {
            // Arrange
            var existingUser = new User { Id = 1, Email = "test@example.com" };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync("test@example.com"))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _authService.UserExistsAsync("test@example.com");

            // Assert
            Assert.True(result);
            _mockUserRepository.Verify(repo => repo.GetByEmailAsync("test@example.com"), Times.Once);
        }

        [Fact]
        public async Task UserExistsAsync_ReturnsFalse_WhenUserDoesNotExist()
        {
            // Arrange
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync("nonexistent@example.com"))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authService.UserExistsAsync("nonexistent@example.com");

            // Assert
            Assert.False(result);
        }
    }
}
