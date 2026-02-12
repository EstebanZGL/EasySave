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
        private readonly string _logFormatFilePath;
        private ObservableCollection<BackupJob> _backupJobs;
        private BackupJob _selectedBackupJob;
        private string _statusMessage;
        private bool _isBusinessSoftwareRunning;
        private bool _isAllJobsSelected;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel(BackupJobManager jobManager, BackupService backupService, 
                          TranslationService translationService, StateManager stateManager)
        {
            _jobManager = jobManager;
            _backupService = backupService;
            _translationService = translationService;
            _stateManager = stateManager;
            _settingsViewModel = new SettingsViewModel();
            _logFormatFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logformat.txt");
            
            // S'abonner aux changements de langue
            _translationService.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == "AllTranslations" || e.PropertyName == nameof(TranslationService.CurrentLanguage))
                {
                    SelectedLanguage = _translationService.CurrentLanguage;
                    // Mettre à jour tous les textes traduits
                    OnPropertyChanged(nameof(AppTitle));
                    OnPropertyChanged(nameof(BackupJobsHeader));
                    OnPropertyChanged(nameof(BackupJobDetailsHeader));
                    OnPropertyChanged(nameof(CreateButtonText));
                    OnPropertyChanged(nameof(ExecuteButtonText));
                    OnPropertyChanged(nameof(EditButtonText));
                    OnPropertyChanged(nameof(DeleteButtonText));
                    OnPropertyChanged(nameof(SettingsButtonText));
                    OnPropertyChanged(nameof(LogFormatLabel));
                    OnPropertyChanged(nameof(NameLabel));
                    OnPropertyChanged(nameof(SourcePathLabel));
                    OnPropertyChanged(nameof(TargetPathLabel));
                    OnPropertyChanged(nameof(TypeLabel));
                    OnPropertyChanged(nameof(JobStatusLabel));
                    OnPropertyChanged(nameof(NotRunningText));
                    OnPropertyChanged(nameof(SelectAllText));
                    OnPropertyChanged(nameof(ExecuteSelectedJobsText));
                    OnPropertyChanged(nameof(LastBackupLabel));
                }
            };
            
            _selectedLanguage = _translationService.CurrentLanguage;
            _selectedLogFormat = LoadLogFormat() ?? "JSON"; // Default log format
            
            LoadBackupJobs();
            
            // Initialize commands
            CreateBackupJobCommand = new RelayCommand(_ => OpenCreateBackupJobDialog());
            EditBackupJobCommand = new RelayCommand(_ => OpenEditBackupJobDialog(), _ => SelectedBackupJob != null);
            DeleteBackupJobCommand = new RelayCommand(async _ => await DeleteBackupJob(), _ => SelectedBackupJob != null);
            ExecuteBackupJobCommand = new RelayCommand(async _ => await ExecuteBackupJob(), _ => SelectedBackupJob != null && !IsBusinessSoftwareRunning);
            ChangeLanguageCommand = new RelayCommand(_ => ChangeLanguage());
            ChangeLogFormatCommand = new RelayCommand(_ => ChangeLogFormat());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
            ExecuteSelectedJobsCommand = new RelayCommand(async _ => await ExecuteSelectedJobs(), _ => HasSelectedJobs && !IsBusinessSoftwareRunning);
            
            // Start business software monitoring
            StartBusinessSoftwareMonitoring();
        }
        
        // Propriétés pour les textes traduits
        public string AppTitle => _translationService.GetTranslation("app_title");
        public string BackupJobsHeader => _translationService.GetTranslation("backup_jobs");
        public string BackupJobDetailsHeader => _translationService.GetTranslation("backup_job_details");
        public string CreateButtonText => _translationService.GetTranslation("menu_create");
        public string ExecuteButtonText => _translationService.GetTranslation("menu_execute");
        public string EditButtonText => _translationService.GetTranslation("menu_edit");
        public string DeleteButtonText => _translationService.GetTranslation("menu_delete");
        public string SettingsButtonText => _translationService.GetTranslation("menu_settings");
        public string LogFormatLabel => _translationService.GetTranslation("log_format");
        public string NameLabel => _translationService.GetTranslation("name");
        public string SourcePathLabel => _translationService.GetTranslation("source_path");
        public string TargetPathLabel => _translationService.GetTranslation("target_path");
        public string TypeLabel => _translationService.GetTranslation("type");
        public string JobStatusLabel => _translationService.GetTranslation("job_status");
        public string NotRunningText => _translationService.GetTranslation("not_running");
        public string SelectAllText => _translationService.GetTranslation("select_all") ?? "Select All";
        public string ExecuteSelectedJobsText => _translationService.GetTranslation("execute_selected") ?? "Execute Selected";
        public string LastBackupLabel => _translationService.GetTranslation("last_backup");

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

        public bool IsAllJobsSelected
        {
            get => _isAllJobsSelected;
            set
            {
                if (_isAllJobsSelected != value)
                {
                    _isAllJobsSelected = value;
                    OnPropertyChanged();
                    
                    // Update all jobs' selection state
                    if (BackupJobs != null)
                    {
                        foreach (var job in BackupJobs)
                        {
                            job.IsSelected = value;
                        }
                    }
                    
                    OnPropertyChanged(nameof(HasSelectedJobs));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        
        public bool HasSelectedJobs => BackupJobs != null && BackupJobs.Any(job => job.IsSelected);

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
                        StatusMessage = _translationService.GetTranslation("status_ready");
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
        public ICommand ExecuteSelectedJobsCommand { get; }

        private void LoadBackupJobs()
        {
            var jobs = _jobManager.GetJobs();
            BackupJobs = new ObservableCollection<BackupJob>(jobs);
            
            // Subscribe to IsSelected property changes for each job
            foreach (var job in BackupJobs)
            {
                job.PropertyChanged += (sender, e) => 
                {
                    if (e.PropertyName == nameof(BackupJob.IsSelected))
                    {
                        OnPropertyChanged(nameof(HasSelectedJobs));
                        CommandManager.InvalidateRequerySuggested();
                        
                        // Update IsAllJobsSelected if needed
                        if (!job.IsSelected && IsAllJobsSelected)
                        {
                            _isAllJobsSelected = false;
                            OnPropertyChanged(nameof(IsAllJobsSelected));
                        }
                        else if (BackupJobs.All(j => j.IsSelected) && !IsAllJobsSelected)
                        {
                            _isAllJobsSelected = true;
                            OnPropertyChanged(nameof(IsAllJobsSelected));
                        }
                    }
                };
            }
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
                    
                    // Mettre à jour la date de dernière sauvegarde
                    SelectedBackupJob.LastBackupTime = DateTime.Now;
                    
                    // Persister la mise à jour dans le fichier JSON
                    await _jobManager.UpdateJob(SelectedBackupJob);
                    
                    StatusMessage = $"Backup job '{SelectedBackupJob.JobName}' completed successfully.";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error executing backup job: {ex.Message}";
                    WPF.MessageBox.Show($"Error executing backup job: {ex.Message}", "Error", WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Error);
                }
            }
        }
        
        private async Task ExecuteSelectedJobs()
        {
            if (_settingsViewModel.IsBusinessSoftwareRunning())
            {
                StatusMessage = $"Cannot start backup: Business software ({_settingsViewModel.BusinessSoftwareName}) is running.";
                WPF.MessageBox.Show($"Cannot start backup: Business software ({_settingsViewModel.BusinessSoftwareName}) is running.", 
                    "Business Software Running", WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Warning);
                return;
            }
            
            var selectedJobs = BackupJobs.Where(job => job.IsSelected).ToList();
            if (selectedJobs.Count == 0) return;
            
            int successCount = 0;
            int failCount = 0;
            
            StatusMessage = $"Executing {selectedJobs.Count} selected backup jobs...";
            
            foreach (var job in selectedJobs)
            {
                try
                {
                    await _backupService.ExecuteBackupJobAsync(job);
                    
                    // Mettre à jour la date de dernière sauvegarde
                    job.LastBackupTime = DateTime.Now;
                    
                    // Persister la mise à jour dans le fichier JSON
                    await _jobManager.UpdateJob(job);
                    
                    successCount++;
                }
                catch (Exception ex)
                {
                    failCount++;
                    Debug.WriteLine($"Error executing backup job '{job.JobName}': {ex.Message}");
                }
            }
            
            if (failCount == 0)
            {
                StatusMessage = $"All {successCount} backup jobs completed successfully.";
            }
            else
            {
                StatusMessage = $"{successCount} jobs completed successfully, {failCount} jobs failed.";
                WPF.MessageBox.Show($"{failCount} backup jobs failed to execute. Check the logs for details.", 
                    "Backup Execution Error", WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Warning);
            }
        }

        private void ChangeLanguage()
        {
            _translationService.ToggleLanguage();
            // La mise à jour de SelectedLanguage et des autres propriétés traduites 
            // se fait via l'événement PropertyChanged du TranslationService
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
                File.WriteAllText(_logFormatFilePath, _selectedLogFormat);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving log format: {ex.Message}");
            }
        }
        
        private string LoadLogFormat()
        {
            try
            {
                if (File.Exists(_logFormatFilePath))
                {
                    string format = File.ReadAllText(_logFormatFilePath).Trim();
                    if (format == "XML" || format == "JSON")
                    {
                        return format;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading log format: {ex.Message}");
            }
            
            return null;
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