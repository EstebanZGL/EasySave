using EasySave.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EasySave.ViewModels;

namespace EasySave.Tests.Services
{
    public class CryptoServiceTests
    {
        [Fact]
        public void ShouldEncrypt_WithMatchingExtension_ReturnsTrue()
        {
            // Arrange
            var settingsViewModelMock = new Mock<SettingsViewModel>();
            settingsViewModelMock.Setup(s => s.ExtensionsToEncrypt).Returns(new[] { ".txt", ".docx", ".pdf" });
            
            var cryptoService = new CryptoService(settingsViewModelMock.Object);
            
            // Act
            bool shouldEncrypt = cryptoService.ShouldEncrypt("test.txt");
            
            // Assert
            Assert.True(shouldEncrypt);
        }
        
        [Fact]
        public void ShouldEncrypt_WithNonMatchingExtension_ReturnsFalse()
        {
            // Arrange
            var settingsViewModelMock = new Mock<SettingsViewModel>();
            settingsViewModelMock.Setup(s => s.ExtensionsToEncrypt).Returns(new[] { ".txt", ".docx", ".pdf" });
            
            var cryptoService = new CryptoService(settingsViewModelMock.Object);
            
            // Act
            bool shouldEncrypt = cryptoService.ShouldEncrypt("test.jpg");
            
            // Assert
            Assert.False(shouldEncrypt);
        }
        
        [Fact]
        public void ShouldEncrypt_WithNoExtensionsToEncrypt_ReturnsFalse()
        {
            // Arrange
            var settingsViewModelMock = new Mock<SettingsViewModel>();
            settingsViewModelMock.Setup(s => s.ExtensionsToEncrypt).Returns(Array.Empty<string>());
            
            var cryptoService = new CryptoService(settingsViewModelMock.Object);
            
            // Act
            bool shouldEncrypt = cryptoService.ShouldEncrypt("test.txt");
            
            // Assert
            Assert.False(shouldEncrypt);
        }
        
        [Fact]
        public void ShouldEncrypt_WithNullExtensionsToEncrypt_ReturnsFalse()
        {
            // Arrange
            var settingsViewModelMock = new Mock<SettingsViewModel>();
            settingsViewModelMock.Setup(s => s.ExtensionsToEncrypt).Returns((string[])null);
            
            var cryptoService = new CryptoService(settingsViewModelMock.Object);
            
            // Act
            bool shouldEncrypt = cryptoService.ShouldEncrypt("test.txt");
            
            // Assert
            Assert.False(shouldEncrypt);
        }
        
        [Fact]
        public void ShouldEncrypt_WithNullFilePath_ReturnsFalse()
        {
            // Arrange
            var settingsViewModelMock = new Mock<SettingsViewModel>();
            settingsViewModelMock.Setup(s => s.ExtensionsToEncrypt).Returns(new[] { ".txt", ".docx", ".pdf" });
            
            var cryptoService = new CryptoService(settingsViewModelMock.Object);
            
            // Act
            bool shouldEncrypt = cryptoService.ShouldEncrypt(null);
            
            // Assert
            Assert.False(shouldEncrypt);
        }
    }
}