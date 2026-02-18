using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization; // Ajout de cette directive pour JsonIgnore
using System.Threading.Tasks;
using EasySave.Models;
using System.Threading;
 
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
                catch (Exception ex)
                {
                    // Log the error but continue
                    Console.WriteLine($"Error loading state file: {ex.Message}");
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
            int totalFiles = 0,
            long totalSize = 0,
            int filesRemaining = 0,
            long sizeRemaining = 0,
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
                jobState.StateEnum = state; // This will also update the State string property
                
                // Only update these values if they are provided (non-default)
                if (totalFiles > 0) jobState.TotalFilesToCopy = totalFiles;
                if (totalSize > 0) jobState.TotalFilesSize = totalSize;
                
                // Always update remaining values
                jobState.NbFilesLeftToDo = filesRemaining;
                jobState.SizeRemaining = sizeRemaining;
                
                // Update file paths if provided
                if (currentSourceFile != null) jobState.SourceFilePath = currentSourceFile;
                if (currentTargetFile != null) jobState.TargetFilePath = currentTargetFile;
               
                // Calculate progress safely to avoid division by zero
                if (jobState.TotalFilesToCopy > 0)
                {
                    jobState.Progression = (jobState.TotalFilesToCopy - jobState.NbFilesLeftToDo) * 100 / jobState.TotalFilesToCopy;
                }
                
                // Update timestamps based on state
                if (state == BackupState.Active && !jobState.StartTime.HasValue)
                {
                    jobState.StartTime = DateTime.Now;
                    jobState.EndTime = null;
                }
                else if ((state == BackupState.Completed || state == BackupState.Failed || state == BackupState.Canceled) && !jobState.EndTime.HasValue)
                {
                    jobState.EndTime = DateTime.Now;
                }
                else if (state == BackupState.Paused)
                {
                    // Don't update timestamps for pause state
                }

                await SaveStateAsync();
            }
            finally
            {
                _stateLock.Release();
            }
        }
 
        // Gets the state of a specific backup job
        public async Task<BackupJobState> GetStateAsync(string name)
        {
            await _stateLock.WaitAsync();
            try
            {
                if (_states.TryGetValue(name, out var state))
                {
                    return state;
                }
                return null;
            }
            finally
            {
                _stateLock.Release();
            }
        }

        // Saves all states to the state file
        public async Task SaveAllStatesAsync()
        {
            await _stateLock.WaitAsync();
            try
            {
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
                // Format the state data according to the specified format
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = null // Preserve property names as they are
                };
               
                string json = JsonSerializer.Serialize(_states.Values, options);
               
                // Use atomic file write to prevent corruption
                string tempFile = Path.GetTempFileName();
                await File.WriteAllTextAsync(tempFile, json);
                File.Move(tempFile, _stateFilePath, true);
            }
            catch (Exception ex)
            {
                // Log the error but continue
                Console.WriteLine($"Error saving state file: {ex.Message}");
            }
        }
    }
 
    // Represents the state of a backup job
    public class BackupJobState
    {
        // Name of the backup job
        public string Name { get; set; }
       
        // Last update timestamp
        [JsonIgnore]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
       
        // Current source file being processed
        public string SourceFilePath { get; set; } = string.Empty;
       
        // Current target file being processed
        public string TargetFilePath { get; set; } = string.Empty;
       
        // Backing field for state
        private BackupState _state = BackupState.NotStarted;
        
        // Current state of the backup job (formatted as string)
        public string State
        {
            get
            {
                switch (_state)
                {
                    case BackupState.Completed: return "END";
                    case BackupState.Active: return "ACTIVE";
                    case BackupState.Paused: return "PAUSED";
                    case BackupState.Canceled: return "CANCELED";
                    case BackupState.Failed: return "ERROR";
                    case BackupState.NotStarted: return "IDLE";
                    case BackupState.Inactive: return "IDLE";
                    default: return _state.ToString().ToUpper();
                }
            }
            set
            {
                switch (value?.ToUpper())
                {
                    case "END":
                        _state = BackupState.Completed;
                        break;
                    case "ACTIVE":
                        _state = BackupState.Active;
                        break;
                    case "PAUSED":
                        _state = BackupState.Paused;
                        break;
                    case "CANCELED":
                        _state = BackupState.Canceled;
                        break;
                    case "ERROR":
                        _state = BackupState.Failed;
                        break;
                    case "IDLE":
                        _state = BackupState.NotStarted;
                        break;
                    default:
                        if (Enum.TryParse<BackupState>(value, true, out var result))
                            _state = result;
                        break;
                }
            }
        }
       
        // Enum representation of the state
        [JsonIgnore]
        public BackupState StateEnum
        {
            get { return _state; }
            set { _state = value; }
        }
       
        // Total number of files
        public int TotalFilesToCopy { get; set; }
       
        // Total size in bytes
        public long TotalFilesSize { get; set; }
       
        // Number of files remaining
        public int NbFilesLeftToDo { get; set; }
       
        // Progress percentage (0-100)
        public int Progression { get; set; }
       
        // Size remaining in bytes
        public long SizeRemaining { get; set; }

        // Start time of the backup job
        [JsonIgnore]
        public DateTime? StartTime { get; set; }

        // End time of the backup job
        [JsonIgnore]
        public DateTime? EndTime { get; set; }

        // Properties needed for compatibility with tests
        [JsonIgnore]
        public string JobName { get => Name; set => Name = value; }

        [JsonIgnore]
        public string Status { get => State; set => State = value; }

        [JsonIgnore]
        public int TotalFiles { get => TotalFilesToCopy; set => TotalFilesToCopy = value; }

        [JsonIgnore]
        public int TotalFilesRemaining { get => NbFilesLeftToDo; set => NbFilesLeftToDo = value; }

        [JsonIgnore]
        public long TotalSize { get => TotalFilesSize; set => TotalFilesSize = value; }

        [JsonIgnore]
        public long TotalSizeRemaining { get => SizeRemaining; set => SizeRemaining = value; }

        [JsonIgnore]
        public string CurrentFile { get => SourceFilePath; set => SourceFilePath = value; }

        [JsonIgnore]
        public string CurrentFileDestination { get => TargetFilePath; set => TargetFilePath = value; }

        [JsonIgnore]
        public int Progress { get => Progression; set => Progression = value; }
    }
}