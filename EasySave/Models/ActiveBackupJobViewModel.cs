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
        private bool _isPaused;

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
                    
                    // Update IsPaused based on the status - using case insensitive comparison
                    IsPaused = string.Equals(_status, JobStatus.Paused, System.StringComparison.OrdinalIgnoreCase);
                    
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
                    // Quand le fichier courant change, mettre à jour DisplayStatus
                    OnPropertyChanged(nameof(DisplayStatus));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the job is paused
        /// </summary>
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

        /// <summary>
        /// Gets a display-friendly status
        /// </summary>
        public string DisplayStatus 
        { 
            get
            {
                // Si le statut est null ou vide ou IDLE, montrer "En cours" si un fichier est en cours de traitement
                if (string.IsNullOrEmpty(Status) || Status?.ToUpper() == "IDLE")
                {
                    if (!string.IsNullOrEmpty(CurrentFile))
                    {
                        return JobStatus.Running; // Utiliser la constante
                    }
                    return "Idle"; // Afficher Idle si pas de fichier en cours
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