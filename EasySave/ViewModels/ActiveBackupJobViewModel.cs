using EasySave.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.ViewModels
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
        private readonly TranslationService _translationService;

        /// <summary>
        /// Creates a new instance of ActiveBackupJobViewModel
        /// </summary>
        public ActiveBackupJobViewModel()
        {
            // Default constructor
        }

        /// <summary>
        /// Creates a new instance of ActiveBackupJobViewModel with translation service
        /// </summary>
        /// <param name="translationService">The translation service to use</param>
        public ActiveBackupJobViewModel(TranslationService translationService)
        {
            _translationService = translationService;
            if (_translationService != null)
            {
                _translationService.PropertyChanged += OnTranslationServicePropertyChanged;
            }
        }

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
                    
                    // Update IsPaused based on status - COMMENTÉ pour éviter les mises à jour automatiques
                    // IsPaused = (_status == "Paused");
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
                    OnPropertyChanged(nameof(PauseResumeButtonText)); // Mettre à jour le texte du bouton
                }
            }
        }

        /// <summary>
        /// Gets the translated label for "Current File"
        /// </summary>
        public string CurrentFileLabel => GetTranslation("current_file");

        /// <summary>
        /// Gets the translated label for "Progress"
        /// </summary>
        public string ProgressLabel => GetTranslation("progress");

        /// <summary>
        /// Gets the translated text for the pause/resume button based on the current state
        /// </summary>
        public string PauseResumeButtonText => IsPaused ? 
            GetTranslation("resume") : 
            GetTranslation("pause");

        /// <summary>
        /// Gets the translated text for the stop button
        /// </summary>
        public string StopButtonText => GetTranslation("stop");

        /// <summary>
        /// Event raised when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        /// <summary>
        /// Méthode publique pour déclencher manuellement les notifications de changement de propriété
        /// </summary>
        /// <param name="propertyName">Le nom de la propriété qui a changé</param>
        public void NotifyPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Event handler for translation service property changes
        /// </summary>
        private void OnTranslationServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentLanguage" || e.PropertyName == "AllTranslations")
            {
                OnPropertyChanged(nameof(CurrentFileLabel));
                OnPropertyChanged(nameof(ProgressLabel));
                OnPropertyChanged(nameof(PauseResumeButtonText));
                OnPropertyChanged(nameof(StopButtonText));
            }
        }

        /// <summary>
        /// Gets a translation for a key
        /// </summary>
        private string GetTranslation(string key)
        {
            return _translationService?.GetTranslation(key) ?? key;
        }
    }
}