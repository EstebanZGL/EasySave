using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using EasySave.Models;
using EasySave.Commands;

namespace EasySave.ViewModels
{
    /// <summary>
    /// View model for a backup job
    /// </summary>
    public class BackupJobViewModel : ViewModelBase
    {
        private readonly BackupJob _backupJob;
        private bool _isSelected;
        private DateTime? _lastBackupTime;
        
        /// <summary>
        /// Creates a new instance of the BackupJobViewModel
        /// </summary>
        /// <param name="backupJob">The backup job model</param>
        public BackupJobViewModel(BackupJob backupJob)
        {
            _backupJob = backupJob;
            _lastBackupTime = backupJob.LastBackupTime;
        }
        
        /// <summary>
        /// Gets the backup job model
        /// </summary>
        public BackupJob BackupJob => _backupJob;
        
        /// <summary>
        /// Gets the job name
        /// </summary>
        public string JobName => _backupJob.JobName;
        
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
        /// Gets or sets the description
        /// </summary>
        public string Description
        {
            get => _backupJob.Description;
            set
            {
                if (_backupJob.Description != value)
                {
                    _backupJob.Description = value;
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
            set
            {
                if (SetProperty(ref _lastBackupTime, value))
                {
                    _backupJob.LastBackupTime = value;
                    OnPropertyChanged(nameof(LastBackupTimeDisplay));
                }
            }
        }
        
        /// <summary>
        /// Gets the last backup time as a formatted string
        /// </summary>
        public string LastBackupTimeDisplay => LastBackupTime.HasValue ? LastBackupTime.Value.ToString("g") : "Never";

        /// <summary>
        /// Gets the creation date
        /// </summary>
        public DateTime CreatedAt => _backupJob.CreatedAt;

        /// <summary>
        /// Gets the creation date as formatted string
        /// </summary>
        public string CreatedAtDisplay => _backupJob.CreatedAtDisplay;
        
        /// <summary>
        /// Gets or sets whether the job is selected
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
