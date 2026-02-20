using EasySave.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using System.Text.Json;
using EasySave.Models;
using System.Collections.Generic;

namespace EasySave.Tests.Services
{
    public class StateManagerTests
    {
        private readonly string testStateFile = "test_state.json";
        
        // Clean up after tests
        public StateManagerTests()
        {
            if (File.Exists(testStateFile))
            {
                File.Delete(testStateFile);
            }
        }
        
        [Fact]
        public async Task UpdateStateAsync_ShouldUpdateState()
        {
            // Arrange
            var stateManager = new StateManager(testStateFile);
            var jobName = "TestJob";
            
            // Act
            await stateManager.UpdateStateAsync(
                jobName,
                BackupState.Active, // Using Active/InProgress
                100, // totalFiles
                1000, // totalSize
                50, // totalFilesRemaining
                500, // totalSizeRemaining
                "test.txt", // currentFile
                "dest/test.txt" // currentFileDestination
            );
            
            // Assert
            var loadedState = await stateManager.GetStateAsync(jobName);
            Assert.NotNull(loadedState);
            Assert.Equal(jobName, loadedState.Name);
            Assert.Equal("ACTIVE", loadedState.State);
            Assert.Equal(100, loadedState.TotalFilesToCopy);
            Assert.Equal(50, loadedState.NbFilesLeftToDo);
            Assert.Equal(1000, loadedState.TotalFilesSize);
            Assert.Equal(500, loadedState.SizeRemaining);
            Assert.Equal("test.txt", loadedState.SourceFilePath);
            Assert.Equal(50, loadedState.Progression);
        }
        
        [Fact]
        public async Task GetStateAsync_WithNonExistentJob_ReturnsNull()
        {
            // Arrange
            var stateManager = new StateManager(testStateFile);
            
            // Act
            var state = await stateManager.GetStateAsync("NonExistentJob");
            
            // Assert
            Assert.Null(state);
        }
        
        [Fact]
        public async Task SaveAllStatesAsync_ShouldCreateStateFile()
        {
            // Arrange
            var stateManager = new StateManager(testStateFile);
            
            // Create a test job state first
            await stateManager.UpdateStateAsync(
                "TestJob",
                BackupState.Active, // Using Active/InProgress
                100, // totalFiles
                1000, // totalSize
                50, // totalFilesRemaining
                500, // totalSizeRemaining
                "test.txt", // currentFile
                "dest/test.txt" // currentFileDestination
            );
            
            // Act
            await stateManager.SaveAllStatesAsync();
            
            // Assert
            Assert.True(File.Exists(testStateFile));
            var fileContent = await File.ReadAllTextAsync(testStateFile);
            // Spécifier explicitement la classe BackupJobState à utiliser
            var loadedStates = JsonSerializer.Deserialize<List<EasySave.Services.BackupJobState>>(fileContent);
            Assert.NotNull(loadedStates);
            Assert.Contains(loadedStates, s => s.Name == "TestJob");
            Assert.Equal("ACTIVE", loadedStates.Find(s => s.Name == "TestJob").State);
        }
    }
}