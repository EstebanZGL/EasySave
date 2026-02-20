using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using EasySave.Models;
using EasySave.Services;
using EasySave.Views;

namespace EasySave.Views
{
    /// <summary>
    /// Interaction logic for BackupJobDialog.xaml
    /// </summary>
    public partial class BackupJobDialog : Window, INotifyPropertyChanged
    {
        private string _name;
        private string _sourcePath;
        private string _targetPath;
        private bool _isCompleteType = true;
        private bool _isDifferentialType;
        private string _validationMessage;
        private bool _useDefaultTargetPath = true; // Par défaut, utiliser le chemin par défaut
        private readonly TranslationService _translationService;
        private readonly bool _isEditMode;

        public BackupJobDialog()
        {
            InitializeComponent();
            _translationService = TranslationServiceProvider.GetTranslationService();
            _translationService.PropertyChanged += TranslationService_PropertyChanged;
            DataContext = this;
        }

        public BackupJobDialog(BackupJob job) : this()
        {
            _isEditMode = true;
            JobName = job.JobName;
            SourcePath = job.SourcePath;
            
            // Vérifier si le chemin cible est un chemin par défaut
            string defaultPath = GetDefaultTargetPath(job.JobName);
            if (job.TargetPath.Equals(defaultPath, StringComparison.OrdinalIgnoreCase))
            {
                UseDefaultTargetPath = true;
                TargetPath = defaultPath;
            }
            else
            {
                UseDefaultTargetPath = false;
                TargetPath = job.TargetPath;
            }
            
            IsCompleteType = job.Type == BackupType.Complete;
            IsDifferentialType = job.Type == BackupType.Differential;
        }

        private void TranslationService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Mettre à jour toutes les propriétés liées aux traductions
            if (e.PropertyName == "AllTranslations" || e.PropertyName == "CurrentLanguage")
            {
                OnPropertyChanged(nameof(DialogTitle));
                OnPropertyChanged(nameof(NameLabel));
                OnPropertyChanged(nameof(SourcePathLabel));
                OnPropertyChanged(nameof(TargetPathLabel));
                OnPropertyChanged(nameof(BackupTypeLabel));
                OnPropertyChanged(nameof(BrowseButtonText));
                OnPropertyChanged(nameof(UseDefaultLocationText));
                OnPropertyChanged(nameof(CompleteTypeText));
                OnPropertyChanged(nameof(DifferentialTypeText));
                OnPropertyChanged(nameof(SaveButtonText));
                OnPropertyChanged(nameof(CancelButtonText));
                OnPropertyChanged(nameof(DefaultTargetPathDisplay));
            }
        }

        // Propriétés pour les traductions
        public string DialogTitle => _isEditMode 
            ? _translationService.GetTranslation("edit_backup_job") 
            : _translationService.GetTranslation("create_backup_job");
        
        public string NameLabel => _translationService.GetTranslation("name");
        public string SourcePathLabel => _translationService.GetTranslation("source_path");
        public string TargetPathLabel => _translationService.GetTranslation("target_path");
        public string BackupTypeLabel => _translationService.GetTranslation("type");
        public string BrowseButtonText => _translationService.GetTranslation("browse");
        public string UseDefaultLocationText => "Use default location";  // À traduire
        public string CompleteTypeText => _translationService.GetTranslation("complete");
        public string DifferentialTypeText => _translationService.GetTranslation("differential");
        public string SaveButtonText => _translationService.GetTranslation("save");
        public string CancelButtonText => _translationService.GetTranslation("cancel");

        public string JobName
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
                // Mettre à jour l'affichage du chemin par défaut quand le nom change
                OnPropertyChanged(nameof(DefaultTargetPathDisplay));
            }
        }

        public string SourcePath
        {
            get => _sourcePath;
            set
            {
                _sourcePath = value;
                OnPropertyChanged();
            }
        }

        public string TargetPath
        {
            get => _targetPath;
            set
            {
                _targetPath = value;
                OnPropertyChanged();
            }
        }

        public bool IsCompleteType
        {
            get => _isCompleteType;
            set
            {
                _isCompleteType = value;
                OnPropertyChanged();
                if (value) IsDifferentialType = !value;
            }
        }

        public bool IsDifferentialType
        {
            get => _isDifferentialType;
            set
            {
                _isDifferentialType = value;
                OnPropertyChanged();
                if (value) IsCompleteType = !value;
            }
        }

        public bool UseDefaultTargetPath
        {
            get => _useDefaultTargetPath;
            set
            {
                _useDefaultTargetPath = value;
                OnPropertyChanged();
                
                // Si on active l'utilisation du chemin par défaut, mettre à jour le chemin cible
                if (value)
                {
                    TargetPath = GetDefaultTargetPath(JobName);
                }
                
                // Mettre à jour l'affichage du chemin par défaut
                OnPropertyChanged(nameof(DefaultTargetPathDisplay));
            }
        }

        public string DefaultTargetPathDisplay
        {
            get
            {
                string path = GetDefaultTargetPath(JobName);
                string defaultLocationText = _translationService.CurrentLanguage == "fr" 
                    ? "Emplacement par défaut: " 
                    : "Default location: ";
                return $"{defaultLocationText}{path}";
            }
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set
            {
                _validationMessage = value;
                OnPropertyChanged();
            }
        }

        public BackupJob BackupJob
        {
            get
            {
                string finalTargetPath = UseDefaultTargetPath ? GetDefaultTargetPath(JobName) : TargetPath;
                
                return new BackupJob
                {
                    JobName = JobName,
                    SourcePath = SourcePath,
                    TargetPath = finalTargetPath,
                    Type = IsCompleteType ? BackupType.Complete : BackupType.Differential
                };
            }
        }

        /// <summary>
        /// Génère le chemin de destination par défaut basé sur le nom du travail
        /// </summary>
        /// <param name="jobName">Nom du travail de sauvegarde</param>
        /// <returns>Le chemin de destination par défaut</returns>
        private string GetDefaultTargetPath(string jobName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saves", "unnamed_backup");
            }
            
            // Créer le chemin de base saves/[Nom de la sauvegarde]
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saves");
            
            // Nettoyer le nom du travail pour qu'il soit valide comme nom de dossier
            string safeName = string.Join("_", jobName.Split(Path.GetInvalidFileNameChars()));
            
            return Path.Combine(basePath, safeName);
        }

        private void BrowseSourceButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SourcePath = dialog.SelectedPath;
            }
        }

        private void BrowseTargetButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TargetPath = dialog.SelectedPath;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            ValidationMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(JobName))
            {
                ValidationMessage = _translationService.GetTranslation("name_required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(SourcePath))
            {
                ValidationMessage = _translationService.GetTranslation("source_required");
                return false;
            }

            if (!Directory.Exists(SourcePath))
            {
                ValidationMessage = _translationService.GetTranslation("source_not_exist");
                return false;
            }

            string finalTargetPath = UseDefaultTargetPath ? GetDefaultTargetPath(JobName) : TargetPath;

            if (string.IsNullOrWhiteSpace(finalTargetPath))
            {
                ValidationMessage = _translationService.GetTranslation("target_required");
                return false;
            }

            // Create target directory if it doesn't exist
            try
            {
                if (!Directory.Exists(finalTargetPath))
                {
                    Directory.CreateDirectory(finalTargetPath);
                }
            }
            catch (Exception ex)
            {
                ValidationMessage = string.Format(_translationService.GetTranslation("target_error"), ex.Message);
                return false;
            }

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}