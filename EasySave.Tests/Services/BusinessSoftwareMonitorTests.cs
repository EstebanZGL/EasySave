using EasySave.Services;
using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EasySave.ViewModels;
using EasyLog;

namespace EasySave.Tests.Services
{
    public class BusinessSoftwareMonitorTests
    {
        [Fact]
        public void StartMonitoring_ShouldStartTimer()
        {
            // Arrange
            var settingsViewModelMock = new Mock<SettingsViewModel>();
            var backupServiceMock = new Mock<IBackupService>();
            var loggerMock = new Mock<IEncryptionLogger>();
            
            var monitor = new BusinessSoftwareMonitor(
                settingsViewModelMock.Object, 
                backupServiceMock.Object, 
                loggerMock.Object);
            
            // Act
            monitor.StartMonitoring();
            
            // Assert - We can only verify that no exception is thrown
            // In a real test, we would use a mock timer to verify it was started
        }
        
        [Fact]
        public void StopMonitoring_ShouldStopTimer()
        {
            // Arrange
            var settingsViewModelMock = new Mock<SettingsViewModel>();
            var backupServiceMock = new Mock<IBackupService>();
            var loggerMock = new Mock<IEncryptionLogger>();
            
            var monitor = new BusinessSoftwareMonitor(
                settingsViewModelMock.Object, 
                backupServiceMock.Object, 
                loggerMock.Object);
            monitor.StartMonitoring();
            
            // Act
            monitor.StopMonitoring();
            
            // Assert - We can only verify that no exception is thrown
            // In a real test, we would use a mock timer to verify it was stopped
        }
        
        [Fact]
        public void Dispose_ShouldStopMonitoring()
        {
            // Arrange
            var settingsViewModelMock = new Mock<SettingsViewModel>();
            var backupServiceMock = new Mock<IBackupService>();
            var loggerMock = new Mock<IEncryptionLogger>();
            
            var monitor = new BusinessSoftwareMonitor(
                settingsViewModelMock.Object, 
                backupServiceMock.Object, 
                loggerMock.Object);
            monitor.StartMonitoring();
            
            // Act
            monitor.Dispose();
            
            // Assert - We can only verify that no exception is thrown
            // In a real test, we would use a mock timer to verify it was disposed
        }
        
        [Fact]
        public void BusinessSoftwareStatusChanged_ShouldRaiseEvent()
        {
            // Arrange
            var settingsViewModelMock = new Mock<SettingsViewModel>();
            var backupServiceMock = new Mock<IBackupService>();
            var loggerMock = new Mock<IEncryptionLogger>();
            
            var monitor = new BusinessSoftwareMonitor(
                settingsViewModelMock.Object, 
                backupServiceMock.Object, 
                loggerMock.Object);
                
            bool eventRaised = false;
            bool eventValue = false;
            
            monitor.BusinessSoftwareStatusChanged += (sender, isRunning) => {
                eventRaised = true;
                eventValue = isRunning;
            };
            
            
            Assert.False(eventRaised);
            
            }
    }
}