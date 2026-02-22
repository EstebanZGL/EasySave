using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace EasySave.Models
{
    // Represents a backup job configuration
    public class BackupJob : INotifyPropertyChanged
    {
        private string _name;
        private string _sourcePath;
        private string _targetPath;
        private string _description;
        private BackupType _type;
        private DateTime _createdAt;
        private DateTime? _lastBackupTime;
        private bool _isSelected;

        // Name of the backup job
        public string JobName
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Name)); // Notify Name property changed too
                }
            }
        }

        // Property that redirects to JobName for backward compatibility
        [JsonIgnore] // To avoid serializing the same data twice
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(JobName)); // Notify JobName property changed too
                }
            }
        }

        // Source directory path
        public string SourcePath
        {
            get => _sourcePath;
            set
            {
                if (_sourcePath != value)
                {
                    _sourcePath = value;
                    OnPropertyChanged();
                }
            }
        }

        // Target directory path
        public string TargetPath
        {
            get => _targetPath;
            set
            {
                if (_targetPath != value)
                {
                    _targetPath = value;
                    OnPropertyChanged();
                }
            }
        }

        // Job description
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        // Type of backup (Complete or Differential)
        public BackupType Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged();
                }
            }
        }

        // Creation date of the job
        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                if (_createdAt != value)
                {
                    _createdAt = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CreatedAtDisplay));
                }
            }
        }

        // Last backup time (nullable)
        public DateTime? LastBackupTime
        {
            get => _lastBackupTime;
            set
            {
                if (_lastBackupTime != value)
                {
                    _lastBackupTime = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LastBackupTimeDisplay));
                }
            }
        }

        // Formatted display of last backup time
        public string LastBackupTimeDisplay
        {
            get
            {
                if (_lastBackupTime.HasValue)
                {
                    return _lastBackupTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                }
                return "Never";
            }
        }

        // Formatted display of creation time
        public string CreatedAtDisplay => _createdAt.ToString("yyyy-MM-dd HH:mm:ss");

        // Selection state for UI (not serialized to JSON)
        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        // Constructor with all parameters
        public BackupJob(string name, string sourcePath, string targetPath, BackupType type)
        {
            _name = name;
            _sourcePath = sourcePath;
            _targetPath = targetPath;
            _description = string.Empty;
            _type = type;
            _createdAt = DateTime.Now;
            _isSelected = false;
        }

        // Default constructor for serialization
        public BackupJob() 
        {
            _description = string.Empty;
            _createdAt = DateTime.Now;
            _isSelected = false;
        }

        // Validates the backup job parameters
        // Returns: True if valid, false otherwise
        public bool Validate()
        {
            // Check if name is not empty
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            // Check if source path exists
            if (string.IsNullOrWhiteSpace(SourcePath))
                return false;

            // Check if target path is specified
            if (string.IsNullOrWhiteSpace(TargetPath))
                return false;

            return true;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Type of backup
    public enum BackupType
    {
        // Complete backup (copies all files)
        Complete,
        
        // Differential backup (copies only new or modified files)
        Differential
    }

    /// <summary>
    /// State of a backup job
    /// </summary>
    public enum BackupState
    {
        /// <summary>
        /// The backup job has not started yet
        /// </summary>
        NotStarted,
        
        /// <summary>
        /// Backup job is inactive
        /// </summary>
        Inactive,
        
        /// <summary>
        /// The backup job is in progress
        /// </summary>
        InProgress,
        
        /// <summary>
        /// Backup job is active (legacy name, same as InProgress)
        /// </summary>
        Active = InProgress,
        
        /// <summary>
        /// The backup job is paused
        /// </summary>
        Paused,
        
        /// <summary>
        /// The backup job has been canceled
        /// </summary>
        Canceled,
        
        /// <summary>
        /// The backup job has been cancelled (legacy spelling)
        /// </summary>
        Cancelled = Canceled,
        
        /// <summary>
        /// The backup job has completed successfully
        /// </summary>
        Completed,
        
        /// <summary>
        /// The backup job has failed
        /// </summary>
        Failed,
        
        /// <summary>
        /// Backup job has failed (legacy name, same as Failed)
        /// </summary>
        Error = Failed
    }
}
