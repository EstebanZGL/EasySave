using EasySave.Models;
using System;
using Xunit;

namespace EasySave.Tests.Models
{
    public class BackupJobTests
    {
        [Fact]
        public void Validate_WithValidParameters_ReturnsTrue()
        {
            // Arrange
            var job = new BackupJob
            {
                Name = "Test Job",
                SourcePath = @"C:\TestSource",
                TargetPath = @"C:\TestTarget",
                Type = BackupType.Complete
            };

            // Act
            bool isValid = job.Validate();

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void Validate_WithEmptyName_ReturnsFalse()
        {
            // Arrange
            var job = new BackupJob
            {
                Name = "",
                SourcePath = @"C:\TestSource",
                TargetPath = @"C:\TestTarget",
                Type = BackupType.Complete
            };

            // Act
            bool isValid = job.Validate();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void Validate_WithEmptySourcePath_ReturnsFalse()
        {
            // Arrange
            var job = new BackupJob
            {
                Name = "Test Job",
                SourcePath = "",
                TargetPath = @"C:\TestTarget",
                Type = BackupType.Complete
            };

            // Act
            bool isValid = job.Validate();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void Validate_WithEmptyTargetPath_ReturnsFalse()
        {
            // Arrange
            var job = new BackupJob
            {
                Name = "Test Job",
                SourcePath = @"C:\TestSource",
                TargetPath = "",
                Type = BackupType.Complete
            };

            // Act
            bool isValid = job.Validate();

            // Assert
            Assert.False(isValid);
        }
    }
}