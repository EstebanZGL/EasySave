using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using EasySave.Models;

namespace EasySave.Services
{
    /// <summary>
    /// Manages the state of backup jobs in real time
    /// </summary>
    public class StateManager
    {
        private readonly string _stateFilePath;
        private readonly Dictionary<string, BackupJobState> _states;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stateFilePath">Path to the state file</param>
        public StateManager(string stateFilePath)
        {
            _stateFilePath = stateFilePath;
            _states = new Dictionary<string, BackupJobState>();
            
            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(_stateFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Load existing state if available
            LoadState();
        }

        /// <summary>
        /// Load the state from the state file
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
                catch
                {
                    // If loading fails, start with an empty state
                }
            }
        }

        /// <summary>
        /// Update the state of a backup job
        /// </summary>
        /// <param name="name">Name of the backup job</param>
        /// <param name="state">State of the backup job</param>
        /// <param name="totalFiles">Total number of files to transfer</param>
        /// <param name="totalSize">Total size of files to transfer in bytes</param>
        /// <param name="filesRemaining">Number of files remaining to transfer</param>
        /// <param name="sizeRemaining">Size of files remaining to transfer in bytes</param>
        /// <param name="currentSourceFile">Current source file being processed</param>
        /// <param name="currentTargetFile">Current target file being processed</param>
        /// <returns>Task representing the asynchronous operation</returns>
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
                jobState = new BackupJobState
                {
                    Name = name
                };
                _states[name] = jobState;
            }

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
        /// Save the current state to the state file
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task SaveStateAsync()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_states.Values, options);
            await File.WriteAllTextAsync(_stateFilePath, json);
        }
    }

    /// <summary>
    /// Class representing the state of a backup job
    /// </summary>
    public class BackupJobState
    {
        /// <summary>
        /// Name of the backup job
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Last update time
        /// </summary>
        public DateTime LastUpdateTime { get; set; }
        
        /// <summary>
        /// State of the backup job
        /// </summary>
        public BackupState State { get; set; }
        
        /// <summary>
        /// Total number of files to transfer
        /// </summary>
        public int TotalFilesCount { get; set; }
        
        /// <summary>
        /// Total size of files to transfer in bytes
        /// </summary>
        public long TotalFilesSize { get; set; }
        
        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public int Progress { get; set; }
        
        /// <summary>
        /// Number of files remaining to transfer
        /// </summary>
        public int FilesRemaining { get; set; }
        
        /// <summary>
        /// Size of files remaining to transfer in bytes
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