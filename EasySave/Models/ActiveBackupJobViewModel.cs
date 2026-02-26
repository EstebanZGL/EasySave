using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.Models
{
     
    /// View model for an active backup job
     
    public class ActiveBackupJobViewModel : INotifyPropertyChanged
    {
        private string _jobName;
        private string _status;
        private int _progress;
        private string _currentFile;
        private bool _isPaused;

         
        /// Gets or sets the name of the job
         
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

         
        /// Gets or sets the status of the job
         
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    
                    // Update IsPaused based on the status - using case insensitive comparison
                    IsPaused = string.Equals(_status, JobStatus.Paused, System.StringComparison.OrdinalIgnoreCase);
                    
                    OnPropertyChanged(nameof(DisplayStatus));
                }
            }
        }

         
        /// Gets or sets the progress of the job (0-100)
         
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

         
        /// Gets or sets the current file being processed
         
        public string CurrentFile
        {
            get => _currentFile;
            set
            {
                if (_currentFile != value)
                {
                    _currentFile = value;
                    OnPropertyChanged();
                    // Quand le fichier courant change, mettre à jour DisplayStatus
                    OnPropertyChanged(nameof(DisplayStatus));
                }
            }
        }

         
        /// Gets or sets whether the job is paused
         
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (_isPaused != value)
                {
                    _isPaused = value;
                    OnPropertyChanged();
                }
            }
        }

         
        /// Gets a display-friendly status
         
        public string DisplayStatus 
        { 
            get
            {
            
                if (string.IsNullOrEmpty(Status) || Status?.ToUpper() == "IDLE")
                {
                    if (!string.IsNullOrEmpty(CurrentFile))
                    {
                        return JobStatus.Running; 
                    }
                    return "Idle"; 
                }
                return Status;
            }
        }

         
        /// Event raised when a property changes
         
        public event PropertyChangedEventHandler PropertyChanged;

         
        /// Raises the PropertyChanged event
         
        /// <param name="propertyName">The name of the property that changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}