using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using EasySave.Models;
using EasySave.Services;
using Microsoft.Win32;

namespace EasySave.Views
{
     
    /// Interaction logic for BackupJobDialog.xaml
     
    public partial class BackupJobDialog : Window, INotifyPropertyChanged
    {
        private string _name;
        private string _sourcePath;
        private string _targetPath;
        private string _description;
        private bool _isCompleteType = true;
        private bool _isDifferentialType;
        private string _validationMessage;
        private DateTime _createdAt;
        private DateTime? _lastBackupTime;
        private bool _useDefaultTargetPath = true; // By default, use the default path
        private readonly TranslationService _translationService;

        public BackupJobDialog()
        {
            _createdAt = DateTime.Now;
            _lastBackupTime = null;
            InitializeComponent();
            DataContext = this;
            
            // Get the translation service from the application by explicitly specifying System.Windows.Application
            _translationService = ((App)System.Windows.Application.Current).ServiceProvider.GetService(typeof(TranslationService)) as TranslationService;
        }

        public BackupJobDialog(BackupJob job) : this()
        {
            JobName = job.JobName;
            SourcePath = job.SourcePath;
            
            // Check if the target path is a default path
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
            _createdAt = job.CreatedAt;
            _lastBackupTime = job.LastBackupTime;
        }

        public string JobName
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
                // Update the default path display when the name changes
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

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
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
                
                // If default path usage is enabled, update the target path
                if (value)
                {
                    TargetPath = GetDefaultTargetPath(JobName);
                }
                
                // Update the default path display
                OnPropertyChanged(nameof(DefaultTargetPathDisplay));
            }
        }

        public string DefaultTargetPathDisplay
        {
            get
            {
                string path = GetDefaultTargetPath(JobName);
                return $"{GetTranslation("default_location")}: {path}";
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
                string finalTargetPath = ResolveTargetPath();
                
                return new BackupJob
                {
                    JobName = JobName,
                    SourcePath = SourcePath,
                    TargetPath = finalTargetPath,
                    Description = Description,
                    CreatedAt = _createdAt,
                    LastBackupTime = _lastBackupTime,
                    Type = IsCompleteType ? BackupType.Complete : BackupType.Differential
                };
            }
        }

        // Translation properties
        public string DialogTitle => IsEditMode ? GetTranslation("edit_backup_job") : GetTranslation("create_backup_job");
        public string NameLabel => GetTranslation("job_name");
        public string SourcePathLabel => GetTranslation("source_path");
        public string TargetPathLabel => GetTranslation("target_path");
        public string UseDefaultLocationText => GetTranslation("use_default_location");
        public string BackupTypeLabel => GetTranslation("backup_type");
        public string CompleteTypeText => GetTranslation("complete");
        public string DifferentialTypeText => GetTranslation("differential");
        public string SaveButtonText => GetTranslation("save");
        public string CancelButtonText => GetTranslation("cancel");
        public string BrowseButtonText => GetTranslation("browse");

        // Property to determine if we are in edit mode
        private bool IsEditMode => !string.IsNullOrEmpty(JobName);

         
        /// Generates the default destination path based on the job name
         
        private string GetDefaultTargetPath(string jobName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saves", "unnamed_backup");
            }
            
            // Create the base path output/saves/[Backup Name]
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saves");
            
            // Clean the job name to make it valid as a folder name
            string safeName = string.Join("_", jobName.Split(Path.GetInvalidFileNameChars()));
            
            return Path.Combine(basePath, safeName);
        }
        private string ResolveTargetPath()
        {
            if (!string.IsNullOrWhiteSpace(TargetPath))
            {
                return TargetPath.Trim();
            }

            return GetDefaultTargetPath(JobName);
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
                ValidationMessage = GetTranslation("error_name_required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(SourcePath))
            {
                ValidationMessage = GetTranslation("error_source_required");
                return false;
            }

            if (!Directory.Exists(SourcePath))
            {
                ValidationMessage = GetTranslation("error_source_not_exist");
                return false;
            }

            string finalTargetPath = ResolveTargetPath();

            if (string.IsNullOrWhiteSpace(finalTargetPath))
            {
                ValidationMessage = GetTranslation("error_target_required");
                return false;
            }

            try
            {
                if (!Directory.Exists(finalTargetPath))
                {
                    Directory.CreateDirectory(finalTargetPath);
                }

                // Persist the resolved target path when user leaves TargetPath empty.
                TargetPath = finalTargetPath;
            }
            catch (Exception ex)
            {
                ValidationMessage = $"{GetTranslation("error_creating_target")}: {ex.Message}";
                return false;
            }

            return true;
        }

        private string GetTranslation(string key)
        {
            return _translationService?.GetTranslation(key) ?? key;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
