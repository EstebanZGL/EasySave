using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace EasySave.Models
{
    // Represents a backup job configuration with source/target paths and type
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

        // Primary name property used for serialization
        public string JobName
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        // Alternative name property for UI binding
        [JsonIgnore]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(JobName));
                }
            }
        }

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

        // Formatted display strings for dates
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

        public string CreatedAtDisplay => _createdAt.ToString("yyyy-MM-dd HH:mm:ss");

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

        public BackupJob() 
        {
            _description = string.Empty;
            _createdAt = DateTime.Now;
            _isSelected = false;
        }

        // Validates that the backup job has all required fields
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            if (string.IsNullOrWhiteSpace(SourcePath))
                return false;

            if (string.IsNullOrWhiteSpace(TargetPath))
                return false;

            return true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Available backup types
    public enum BackupType
    {
        Complete,     // Copy all files from source to destination
        Differential  // Copy only new or modified files
    }

    // Possible states of a backup job
    public enum BackupState
    {
        NotStarted,
        Inactive,
        InProgress,
        Active = InProgress,
        Paused,
        Canceled,
        Cancelled = Canceled,
        Completed,
        Failed,
        Error = Failed
    }
}