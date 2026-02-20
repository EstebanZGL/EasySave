using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using EasySave.Models;
using Microsoft.Win32;

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

        public BackupJobDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public BackupJobDialog(BackupJob job) : this()
        {
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
                return $"Default location: {path}";
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
            
            // Créer le chemin de base output/saves/[Nom de la sauvegarde]
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
                ValidationMessage = "Name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(SourcePath))
            {
                ValidationMessage = "Source path is required.";
                return false;
            }

            if (!Directory.Exists(SourcePath))
            {
                ValidationMessage = "Source directory does not exist.";
                return false;
            }

            string finalTargetPath = UseDefaultTargetPath ? GetDefaultTargetPath(JobName) : TargetPath;

            if (string.IsNullOrWhiteSpace(finalTargetPath))
            {
                ValidationMessage = "Target path is required.";
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
                ValidationMessage = $"Error creating target directory: {ex.Message}";
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