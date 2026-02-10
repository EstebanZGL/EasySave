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

        public BackupJobDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public BackupJobDialog(BackupJob job) : this()
        {
            JobName  = job.JobName ;
            SourcePath = job.SourcePath;
            TargetPath = job.TargetPath;
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
                return new BackupJob
                {
                    JobName = JobName ,
                    SourcePath = SourcePath,
                    TargetPath = TargetPath,
                    Type = IsCompleteType ? BackupType.Complete : BackupType.Differential
                };
            }
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

            if (string.IsNullOrWhiteSpace(JobName ))
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

            if (string.IsNullOrWhiteSpace(TargetPath))
            {
                ValidationMessage = "Target path is required.";
                return false;
            }

            // Create target directory if it doesn't exist
            try
            {
                if (!Directory.Exists(TargetPath))
                {
                    Directory.CreateDirectory(TargetPath);
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