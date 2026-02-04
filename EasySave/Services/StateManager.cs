using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using EasySave.Models;

namespace EasySave.Services
{
    // Manages the real-time state tracking of backup operations
    public class StateManager
    {
        private readonly string _stateFilePath;
        private readonly Dictionary<string, BackupJobState> _states;
        private readonly SemaphoreSlim _stateLock = new SemaphoreSlim(1, 1); // Thread safety for state updates

        // Initializes a new instance of the StateManager
        public StateManager(string stateFilePath)
        {
            _stateFilePath = stateFilePath ?? throw new ArgumentNullException(nameof(stateFilePath));
            _states = new Dictionary<string, BackupJobState>();
            
            // Create directory if needed
            string directory = Path.GetDirectoryName(_stateFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            LoadState();
        }

        // Loads backup job states from the state file
        private void LoadState()
        {
            if (File.Exists(_stateFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_stateFilePath);
                    var states = JsonSerializer.Deserialize<List<BackupJobState>>(json);
                    
                    if (states != null)
                    {
                        foreach (var state in states)
                        {
                            _states[state.Name] = state;
                        }
                    }
                }
                catch (Exception)
                {
                    // Handle errors gracefully
                }
            }
        }

        // Updates the state of a backup job and persists changes
        // name: Name of the backup job
        // state: Current state
        // totalFiles: Total files count
        // totalSize: Total size in bytes
        // filesRemaining: Files remaining
        // sizeRemaining: Size remaining in bytes
        // currentSourceFile: Current source file (optional)
        // currentTargetFile: Current target file (optional)
        public async Task UpdateStateAsync(
            string name,
            BackupState state,
            int totalFiles,
            long totalSize,
            int filesRemaining,
            long sizeRemaining,
            string currentSourceFile = null,
            string currentTargetFile = null)
        {
            await _stateLock.WaitAsync();
            
            try
            {
                if (!_states.TryGetValue(name, out var jobState))
                {
                    jobState = new BackupJobState { Name = name };
                    _states[name] = jobState;
                }

                // Update state properties
                jobState.LastUpdateTime = DateTime.Now;
                jobState.State = state;
                jobState.TotalFilesCount = totalFiles;
                jobState.TotalFilesSize = totalSize;
                jobState.FilesRemaining = filesRemaining;
                jobState.SizeRemaining = sizeRemaining;
                jobState.CurrentSourceFile = currentSourceFile;
                jobState.CurrentTargetFile = currentTargetFile;
                
                // Calculate progress safely to avoid division by zero
                jobState.Progress = totalFiles > 0 ? (totalFiles - filesRemaining) * 100 / totalFiles : 0;

                await SaveStateAsync();
            }
            finally
            {
                _stateLock.Release();
            }
        }

        // Saves the current state to the state file
        private async Task SaveStateAsync()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_states.Values, options);
                
                // Use atomic file write to prevent corruption
                string tempFile = Path.GetTempFileName();
                await File.WriteAllTextAsync(tempFile, json);
                File.Move(tempFile, _stateFilePath, true);
            }
            catch (Exception)
            {
                // Handle errors gracefully
            }
        }
    }

    // Represents the state of a backup job
    public class BackupJobState
    {
        // Name of the backup job
        public string Name { get; set; }
        
        // Last update timestamp
        public DateTime LastUpdateTime { get; set; }
        
        // Current state of the backup job
        public BackupState State { get; set; }
        
        // Total number of files
        public int TotalFilesCount { get; set; }
        
        // Total size in bytes
        public long TotalFilesSize { get; set; }
        
        // Progress percentage (0-100)
        public int Progress { get; set; }
        
        // Number of files remaining
        public int FilesRemaining { get; set; }
        
        // Size remaining in bytes
        public long SizeRemaining { get; set; }
        
        // Current source file being processed
        public string CurrentSourceFile { get; set; }
        
        // Current target file being processed
        public string CurrentTargetFile { get; set; }
    }
}