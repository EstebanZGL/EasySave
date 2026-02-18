using System;
using EasySave.Models;

namespace EasySave.ViewModels
{
    /// <summary>
    /// View model for a backup job
    /// </summary>
    public class BackupJobViewModel : ViewModelBase
    {
        private BackupJob _backupJob;
        private DateTime? _lastBackupTime;
        private bool _isSelected;

        /// <summary>
        /// Creates a new instance of the BackupJobViewModel class
        /// </summary>
        /// <param name="backupJob">The backup job to wrap</param>
        public BackupJobViewModel(BackupJob backupJob)
        {
            _backupJob = backupJob ?? throw new ArgumentNullException(nameof(backupJob));
        }

        /// <summary>
        /// Gets the underlying backup job
        /// </summary>
        public BackupJob BackupJob => _backupJob;

        /// <summary>
        /// Gets or sets the name of the job
        /// </summary>
        public string JobName
        {
            get => _backupJob.JobName;
            set
            {
                if (_backupJob.JobName != value)
                {
                    _backupJob.JobName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the source path
        /// </summary>
        public string SourcePath
        {
            get => _backupJob.SourcePath;
            set
            {
                if (_backupJob.SourcePath != value)
                {
                    _backupJob.SourcePath = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the target path
        /// </summary>
        public string TargetPath
        {
            get => _backupJob.TargetPath;
            set
            {
                if (_backupJob.TargetPath != value)
                {
                    _backupJob.TargetPath = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the backup type
        /// </summary>
        public BackupType Type
        {
            get => _backupJob.Type;
            set
            {
                if (_backupJob.Type != value)
                {
                    _backupJob.Type = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the last backup time
        /// </summary>
        public DateTime? LastBackupTime
        {
            get => _lastBackupTime;
            set => SetProperty(ref _lastBackupTime, value);
        }

        /// <summary>
        /// Gets or sets whether the job is selected
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// Gets a display-friendly string for the backup type
        /// </summary>
        public string TypeDisplay => _backupJob.Type.ToString();

        /// <summary>
        /// Gets a display-friendly string for the last backup time
        /// </summary>
        public string LastBackupTimeDisplay => _lastBackupTime.HasValue 
            ? _lastBackupTime.Value.ToString("g") 
            : "-";
    }
}