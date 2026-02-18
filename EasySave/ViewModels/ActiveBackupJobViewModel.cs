using System;

namespace EasySave.ViewModels
{
    /// <summary>
    /// View model for an active backup job
    /// </summary>
    public class ActiveBackupJobViewModel : ViewModelBase
    {
        private string _jobName;
        private string _status;
        private int _progress;
        private string _currentFile;
        private bool _isPaused;

        /// <summary>
        /// Gets or sets the name of the job
        /// </summary>
        public string JobName
        {
            get => _jobName;
            set => SetProperty(ref _jobName, value);
        }

        /// <summary>
        /// Gets or sets the status of the job
        /// </summary>
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        /// <summary>
        /// Gets or sets the progress of the job (0-100)
        /// </summary>
        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        /// <summary>
        /// Gets or sets the current file being processed
        /// </summary>
        public string CurrentFile
        {
            get => _currentFile;
            set => SetProperty(ref _currentFile, value);
        }

        /// <summary>
        /// Gets or sets whether the job is paused
        /// </summary>
        public bool IsPaused
        {
            get => _isPaused;
            set => SetProperty(ref _isPaused, value);
        }
    }
}