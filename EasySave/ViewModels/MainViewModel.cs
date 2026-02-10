using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EasySave.Models;
using EasySave.Services;
using EasySave.Views;
using WPF = System.Windows;

namespace EasySave.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly BackupJobManager _jobManager;
        private readonly BackupService _backupService;
        private readonly TranslationService _translationService;
        private readonly StateManager _stateManager;
        private readonly SettingsViewModel _settingsViewModel;
        private string _selectedLanguage;
        private string _selectedLogFormat;
        private ObservableCollection<BackupJob> _backupJobs;
        private BackupJob _selectedBackupJob;
        private string _statusMessage;
        private bool _isBusinessSoftwareRunning;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel(BackupJobManager jobManager, BackupService backupService, 
                          TranslationService translationService, StateManager stateManager)
        {
            _jobManager = jobManager;
            _backupService = backupService;
            _translationService = translationService;
            _stateManager = stateManager;
            _settingsViewModel = new SettingsViewModel();
            
            _selectedLanguage = _translationService.CurrentLanguage;
            _selectedLogFormat = "JSON"; // Default log format
            
            LoadBackupJobs();
            
            // Initialize commands
            CreateBackupJobCommand = new RelayCommand(_ => OpenCreateBackupJobDialog());
            EditBackupJobCommand = new RelayCommand(_ => OpenEditBackupJobDialog(), _ => SelectedBackupJob != null);
            DeleteBackupJobCommand = new RelayCommand(async _ => await DeleteBackupJob(), _ => SelectedBackupJob != null);
            ExecuteBackupJobCommand = new RelayCommand(async _ => await ExecuteBackupJob(), _ => SelectedBackupJob != null && !IsBusinessSoftwareRunning);
            ChangeLanguageCommand = new RelayCommand(_ => ChangeLanguage());
            ChangeLogFormatCommand = new RelayCommand(_ => ChangeLogFormat());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
            
            // Start business software monitoring
            StartBusinessSoftwareMonitoring();
        }

        public ObservableCollection<BackupJob> BackupJobs
        {
            get => _backupJobs;
            set
            {
                _backupJobs = value;
                OnPropertyChanged();
            }
        }

        public BackupJob SelectedBackupJob
        {
            get => _selectedBackupJob;
            set
            {
                _selectedBackupJob = value;
                OnPropertyChanged();
                // Update command can execute status
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged();
            }
        }

        public string SelectedLogFormat
        {
            get => _selectedLogFormat;
            set
            {
                _selectedLogFormat = value;
                OnPropertyChanged();
                SaveLogFormat();
            }
        }
        
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsBusinessSoftwareRunning
        {
            get => _isBusinessSoftwareRunning;
            set
            {
                if (_isBusinessSoftwareRunning != value)
                {
                    _isBusinessSoftwareRunning = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                    
                    if (value)
                    {
                        StatusMessage = $"Business software ({_settingsViewModel.BusinessSoftwareName}) is running. Backup operations are paused.";
                    }
                    else
                    {
                        StatusMessage = "Ready";
                    }
                }
            }
        }

        public ICommand CreateBackupJobCommand { get; }
        public ICommand EditBackupJobCommand { get; }
        public ICommand DeleteBackupJobCommand { get; }
        public ICommand ExecuteBackupJobCommand { get; }
        public ICommand ChangeLanguageCommand { get; }
        public ICommand ChangeLogFormatCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        private void LoadBackupJobs()
        {
            var jobs = _jobManager.GetJobs();
            BackupJobs = new ObservableCollection<BackupJob>(jobs);
        }
        
        private void OpenCreateBackupJobDialog()
        {
            var dialog = new BackupJobDialog();
            if (dialog.ShowDialog() == true)
            {
                CreateBackupJob(dialog.BackupJob);
            }
        }
        
        private void OpenEditBackupJobDialog()
        {
            if (SelectedBackupJob == null) return;
            
            var dialog = new BackupJobDialog(SelectedBackupJob);
            if (dialog.ShowDialog() == true)
            {
                // Delete old job and create new one with updated values
                _jobManager.DeleteJob(SelectedBackupJob.JobName);
                CreateBackupJob(dialog.BackupJob);
            }
        }

        private async void CreateBackupJob(BackupJob job)
        {
            try
            {
                await _jobManager.CreateJob(job);
                LoadBackupJobs(); // Reload all jobs
                StatusMessage = $"Backup job '{job.JobName}' created successfully.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error creating backup job: {ex.Message}";
                WPF.MessageBox.Show($"Error creating backup job: {ex.Message}", "Error", WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Error);
            }
        }

        private async Task DeleteBackupJob()
        {
            if (SelectedBackupJob != null)
            {
                var result = WPF.MessageBox.Show($"Are you sure you want to delete the backup job '{SelectedBackupJob.JobName}'?", 
                    "Confirm Deletion", WPF.MessageBoxButton.YesNo, WPF.MessageBoxImage.Question);
                
                if (result == WPF.MessageBoxResult.Yes)
                {
                    try
                    {
                        await _jobManager.DeleteJob(SelectedBackupJob.JobName);
                        BackupJobs.Remove(SelectedBackupJob);
                        SelectedBackupJob = null;
                        StatusMessage = "Backup job deleted successfully.";
                    }
                    catch (Exception ex)
                    {
                        StatusMessage = $"Error deleting backup job: {ex.Message}";
                        WPF.MessageBox.Show($"Error deleting backup job: {ex.Message}", "Error", WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Error);
                    }
                }
            }
        }

        private async Task ExecuteBackupJob()
        {
            if (SelectedBackupJob != null)
            {
                if (_settingsViewModel.IsBusinessSoftwareRunning())
                {
                    StatusMessage = $"Cannot start backup: Business software ({_settingsViewModel.BusinessSoftwareName}) is running.";
                    WPF.MessageBox.Show($"Cannot start backup: Business software ({_settingsViewModel.BusinessSoftwareName}) is running.", 
                        "Business Software Running", WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Warning);
                    return;
                }
                
                try
                {
                    StatusMessage = $"Executing backup job '{SelectedBackupJob.JobName}'...";
                    await _backupService.ExecuteBackupJobAsync(SelectedBackupJob);
                    StatusMessage = $"Backup job '{SelectedBackupJob.JobName}' completed successfully.";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error executing backup job: {ex.Message}";
                    WPF.MessageBox.Show($"Error executing backup job: {ex.Message}", "Error", WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Error);
                }
            }
        }

        private void ChangeLanguage()
        {
            _translationService.ToggleLanguage();
            SelectedLanguage = _translationService.CurrentLanguage;
        }

        private void ChangeLogFormat()
        {
            // Toggle between JSON and XML
            SelectedLogFormat = SelectedLogFormat == "JSON" ? "XML" : "JSON";
        }
        
        private void SaveLogFormat()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logformat.txt");
                File.WriteAllText(configPath, SelectedLogFormat);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving log format: {ex.Message}");
            }
        }
        
        private void LoadLogFormat()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logformat.txt");
                if (File.Exists(configPath))
                {
                    string format = File.ReadAllText(configPath).Trim();
                    if (format == "XML" || format == "JSON")
                    {
                        SelectedLogFormat = format;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading log format: {ex.Message}");
            }
        }
        
        private void OpenSettings()
        {
            var settingsWindow = new SettingsWindow(_settingsViewModel);
            settingsWindow.ShowDialog();
        }
        
        private void StartBusinessSoftwareMonitoring()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    IsBusinessSoftwareRunning = _settingsViewModel.IsBusinessSoftwareRunning();
                    await Task.Delay(2000); // Check every 2 seconds
                }
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Basic implementation of ICommand for our ViewModel
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}