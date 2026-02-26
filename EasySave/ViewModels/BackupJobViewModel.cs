using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using EasySave.Models;
using EasySave.Commands;

namespace EasySave.ViewModels
{
     
    /// View model for a backup job
     
    public class BackupJobViewModel : ViewModelBase
    {
        private readonly BackupJob _backupJob;
        private bool _isSelected;
        private DateTime? _lastBackupTime;
        
         
        /// Creates a new instance of the BackupJobViewModel
         
        public BackupJobViewModel(BackupJob backupJob)
        {
            _backupJob = backupJob;
            _lastBackupTime = backupJob.LastBackupTime;
        }
        
         
        /// Gets the backup job model
         
        public BackupJob BackupJob => _backupJob;
        
         
        /// Gets the job name
         
        public string JobName => _backupJob.JobName;
        
         
        /// Gets or sets the source path
         
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
        
         
        /// Gets or sets the target path
         
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

         
        /// Gets or sets the description
         
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
        
         
        /// Gets or sets the backup type
         
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
        
         
        /// Gets or sets the last backup time
         
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
        
         
        /// Gets the last backup time as a formatted string
         
        public string LastBackupTimeDisplay => LastBackupTime.HasValue ? LastBackupTime.Value.ToString("g") : "Never";

         
        /// Gets the creation date
         
        public DateTime CreatedAt => _backupJob.CreatedAt;

         
        /// Gets the creation date as formatted string
         
        public string CreatedAtDisplay => _backupJob.CreatedAtDisplay;
        
         
        /// Gets or sets whether the job is selected
         
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
