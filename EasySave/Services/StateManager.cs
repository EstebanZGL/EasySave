using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using EasySave.Models;

namespace EasySave.Services
{
    /// <summary>
    /// Manages the real-time state tracking of backup operations.
    /// </summary>
    public class StateManager
    {
        private readonly string _stateFilePath;
        private readonly Dictionary<string, BackupJobState> _states;

        /// <summary>
        /// Initializes a new instance of the StateManager.
        /// </summary>
        /// <param name="stateFilePath">Path to the state file</param>
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

        /// <summary>
        /// Loads backup job states from the state file.
        /// </summary>
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

        /// <summary>
        /// Updates the state of a backup job and persists changes.
        /// </summary>
        /// <param name="name">Name of the backup job</param>
        /// <param name="state">Current state</param>
        /// <param name="totalFiles">Total files count</param>
        /// <param name="totalSize">Total size in bytes</param>
        /// <param name="filesRemaining">Files remaining</param>
        /// <param name="sizeRemaining">Size remaining in bytes</param>
        /// <param name="currentSourceFile">Current source file</param>
        /// <param name="currentTargetFile">Current target file</param>
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
            jobState.Progress = totalFiles > 0 ? (totalFiles - filesRemaining) * 100 / totalFiles : 0;

            await SaveStateAsync();
        }

        /// <summary>
        /// Saves the current state to the state file.
        /// </summary>
        private async Task SaveStateAsync()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_states.Values, options);
                await File.WriteAllTextAsync(_stateFilePath, json);
            }
            catch (Exception)
            {
                // Handle errors gracefully
            }
        }
    }

    /// <summary>
    /// Represents the state of a backup job.
    /// </summary>
    public class BackupJobState
    {
        /// <summary>
        /// Name of the backup job
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime LastUpdateTime { get; set; }
        
        /// <summary>
        /// Current state of the backup job
        /// </summary>
        public BackupState State { get; set; }
        
        /// <summary>
        /// Total number of files
        /// </summary>
        public int TotalFilesCount { get; set; }
        
        /// <summary>
        /// Total size in bytes
        /// </summary>
        public long TotalFilesSize { get; set; }
        
        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public int Progress { get; set; }
        
        /// <summary>
        /// Number of files remaining
        /// </summary>
        public int FilesRemaining { get; set; }
        
        /// <summary>
        /// Size remaining in bytes
        /// </summary>
        public long SizeRemaining { get; set; }
        
        /// <summary>
        /// Current source file being processed
        /// </summary>
        public string CurrentSourceFile { get; set; }
        
        /// <summary>
        /// Current target file being processed
        /// </summary>
        public string CurrentTargetFile { get; set; }
    }
}