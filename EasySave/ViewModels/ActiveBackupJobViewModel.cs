using EasySave.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.ViewModels
{
     
    /// View model for an active backup job
     
    public class ActiveBackupJobViewModel : INotifyPropertyChanged
    {
        private string _jobName;
        private string _status;
        private int _progress;
        private string _currentFile;
        private bool _isPaused;
        private readonly TranslationService _translationService;

         
        /// Creates a new instance of ActiveBackupJobViewModel
         
        public ActiveBackupJobViewModel()
        {
            // Default constructor
        }

         
        /// Creates a new instance of ActiveBackupJobViewModel with translation service
         
        public ActiveBackupJobViewModel(TranslationService translationService)
        {
            _translationService = translationService;
            if (_translationService != null)
            {
                _translationService.PropertyChanged += OnTranslationServicePropertyChanged;
            }
        }

         
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
                    
                    // Update IsPaused based on status - COMMENTÉ pour éviter les mises à jour automatiques
                    // IsPaused = (_status == "Paused");
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
                    OnPropertyChanged(nameof(PauseResumeButtonText)); // Mettre à jour le texte du bouton
                }
            }
        }

         
        /// Gets the translated label for "Current File"
         
        public string CurrentFileLabel => GetTranslation("current_file");

         
        /// Gets the translated label for "Progress"
         
        public string ProgressLabel => GetTranslation("progress");

         
        /// Gets the translated text for the pause/resume button based on the current state
         
        public string PauseResumeButtonText => IsPaused ? 
            GetTranslation("resume") : 
            GetTranslation("pause");

         
        /// Gets the translated text for the stop button
         
        public string StopButtonText => GetTranslation("stop");

         
        /// Event raised when a property changes
         
        public event PropertyChangedEventHandler PropertyChanged;

         
        /// Raises the PropertyChanged event
         
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
         
        /// Méthode publique pour déclencher manuellement les notifications de changement de propriété
         
        public void NotifyPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

         
        /// Event handler for translation service property changes
         
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

         
        /// Gets a translation for a key
         
        private string GetTranslation(string key)
        {
            return _translationService?.GetTranslation(key) ?? key;
        }
    }
}