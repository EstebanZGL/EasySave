using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.Models
{
    /// <summary>
    /// View model for an active backup job
    /// </summary>
    public class ActiveBackupJobViewModel : INotifyPropertyChanged
    {
        private string _jobName;
        private string _status;
        private int _progress;
        private string _currentFile;

        /// <summary>
        /// Gets or sets the name of the job
        /// </summary>
        public string JobName
        {
            get => _jobName;
            set
            {
                if (_jobName != value)
                {
                    _jobName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the status of the job
        /// </summary>
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsPaused));
                    OnPropertyChanged(nameof(DisplayStatus));
                }
            }
        }

        /// <summary>
        /// Gets or sets the progress of the job (0-100)
        /// </summary>
        public int Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current file being processed
        /// </summary>
        public string CurrentFile
        {
            get => _currentFile;
            set
            {
                if (_currentFile != value)
                {
                    _currentFile = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets whether the job is paused
        /// </summary>
        public bool IsPaused => Status?.ToUpper() == "PAUSED" || Status?.ToUpper() == "PAUSING";

        /// <summary>
        /// Gets a display-friendly status
        /// </summary>
        public string DisplayStatus 
        { 
            get
            {
                // Si le statut est null ou IDLE mais qu'on a un fichier en cours, montrer "En cours"
                if ((string.IsNullOrEmpty(Status) || Status?.ToUpper() == "IDLE") && !string.IsNullOrEmpty(CurrentFile))
                {
                    return "En cours";
                }
                return Status;
            }
        }

        /// <summary>
        /// Event raised when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}