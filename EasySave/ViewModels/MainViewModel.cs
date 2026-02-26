using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EasySave.Models;
using EasySave.Services;
using EasySave.Commands;
using EasySave.Views;
using System.Diagnostics;

namespace EasySave.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // Services
        private readonly BackupJobRepository _backupJobRepository;
        private readonly ParallelBackupService _backupService;
        private readonly BusinessSoftwareMonitor _businessSoftwareMonitor;
        private readonly TranslationService _translationService;
        
        // Properties for UI
        private ObservableCollection<BackupJobViewModel> _backupJobs;
        private ObservableCollection<ActiveBackupJobViewModel> _activeJobs;
        private BackupJobViewModel _selectedBackupJob;
        private bool _isAllJobsSelected;
        private bool _isBusinessSoftwareRunning;
        private string _statusMessage;
        private bool _hasSelectedJobs;
        
        // Commands
        public ICommand CreateBackupJobCommand { get; }
        public ICommand EditBackupJobCommand { get; }
        public ICommand DeleteBackupJobCommand { get; }
        public ICommand ExecuteBackupJobCommand { get; }
        public ICommand ExecuteSelectedJobsCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand ChangeLanguageCommand { get; }
        public ICommand PauseResumeJobCommand { get; }
        public ICommand StopJobCommand { get; }

        public MainViewModel(
            BackupJobRepository backupJobRepository,
            ParallelBackupService backupService,
            BusinessSoftwareMonitor businessSoftwareMonitor,
            TranslationService translationService)
        {
            _backupJobRepository = backupJobRepository;
            _backupService = backupService;
            _businessSoftwareMonitor = businessSoftwareMonitor;
            _translationService = translationService;
            
            // Initialize collections
            _backupJobs = new ObservableCollection<BackupJobViewModel>();
            _activeJobs = new ObservableCollection<ActiveBackupJobViewModel>();
            
            // Initialize commands
            CreateBackupJobCommand = new RelayCommand(CreateBackupJob);
            EditBackupJobCommand = new RelayCommand(EditBackupJob, param => SelectedBackupJob != null);
            DeleteBackupJobCommand = new RelayCommand(DeleteBackupJob, param => SelectedBackupJob != null);
            ExecuteBackupJobCommand = new RelayCommand(ExecuteBackupJob, param => SelectedBackupJob != null);
            ExecuteSelectedJobsCommand = new RelayCommand(ExecuteSelectedJobs, param => HasSelectedJobs);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            ChangeLanguageCommand = new RelayCommand(param => ToggleLanguage());
            PauseResumeJobCommand = new RelayCommand(PauseResumeJob, param => true);
            StopJobCommand = new RelayCommand(StopJob);
            
            // Subscribe to events
            _backupService.BackupJobStatusChanged += OnBackupJobStatusChanged;
            _businessSoftwareMonitor.BusinessSoftwareStatusChanged += OnBusinessSoftwareStatusChanged;
            _translationService.PropertyChanged += OnTranslationServicePropertyChanged;
            
            // Load backup jobs
            LoadBackupJobs();
        }

        public ObservableCollection<BackupJobViewModel> BackupJobs
        {
            get => _backupJobs;
            set => SetProperty(ref _backupJobs, value);
        }

        public ObservableCollection<ActiveBackupJobViewModel> ActiveJobs
        {
            get => _activeJobs;
            set => SetProperty(ref _activeJobs, value);
        }

        public BackupJobViewModel SelectedBackupJob
        {
            get => _selectedBackupJob;
            set => SetProperty(ref _selectedBackupJob, value);
        }

        public bool IsAllJobsSelected
        {
            get => _isAllJobsSelected;
            set
            {
                if (SetProperty(ref _isAllJobsSelected, value))
                {
                    foreach (var job in BackupJobs)
                    {
                        job.IsSelected = value;
                    }
                    UpdateHasSelectedJobs();
                }
            }
        }

        public bool IsBusinessSoftwareRunning
        {
            get => _isBusinessSoftwareRunning;
            set => SetProperty(ref _isBusinessSoftwareRunning, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool HasSelectedJobs
        {
            get => _hasSelectedJobs;
            private set => SetProperty(ref _hasSelectedJobs, value);
        }

        // Translation properties
        public string AppTitle => _translationService.GetTranslation("app_title");
        public string SelectedLanguage => _translationService.CurrentLanguage == "en" ? "English" : "Français";
        public string SettingsButtonText => _translationService.GetTranslation("menu_settings");
        public string SelectAllText => _translationService.GetTranslation("select_all");
        public string BackupJobsHeader => _translationService.GetTranslation("backup_jobs");
        public string NameLabel => _translationService.GetTranslation("name");
        public string SourcePathLabel => _translationService.GetTranslation("source_path");
        public string TargetPathLabel => _translationService.GetTranslation("target_path");
        public string TypeLabel => _translationService.GetTranslation("type");
        public string DescriptionLabel => _translationService.GetTranslation("description");
        public string LastBackupLabel => _translationService.GetTranslation("last_backup");
        public string CreatedLabel => _translationService.GetTranslation("created");
        public string BackupJobDetailsHeader => _translationService.GetTranslation("backup_job_details");
        public string JobStatusLabel => _translationService.GetTranslation("job_status");
        public string NotRunningText => _translationService.GetTranslation("not_running");
        public string ExecuteButtonText => _translationService.GetTranslation("menu_execute");
        public string EditButtonText => _translationService.GetTranslation("menu_edit");
        public string DeleteButtonText => _translationService.GetTranslation("menu_delete");
        public string ActiveJobsHeader => _translationService.GetTranslation("job_status");
        public string CreateButtonText => _translationService.GetTranslation("menu_create");
        public string ExecuteSelectedJobsText => _translationService.GetTranslation("execute_selected");
        public string PauseButtonText => _translationService.GetTranslation("pause");
        public string ResumeButtonText => _translationService.GetTranslation("resume");
        public string StopButtonText => _translationService.GetTranslation("stop");
        public string CurrentFileLabel => _translationService.GetTranslation("current_file");
        public string ProgressLabel => _translationService.GetTranslation("progress");
        
        private void LoadBackupJobs()
        {
            BackupJobs.Clear();
            
            foreach (var job in _backupJobRepository.GetAllBackupJobs())
            {
                var jobViewModel = new BackupJobViewModel(job);
                jobViewModel.PropertyChanged += OnBackupJobViewModelPropertyChanged;
                BackupJobs.Add(jobViewModel);
            }
            
            UpdateHasSelectedJobs();
        }
        
        private void UpdateHasSelectedJobs()
        {
            HasSelectedJobs = BackupJobs.Any(j => j.IsSelected);
        }
        
        private void OnBackupJobViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BackupJobViewModel.IsSelected))
            {
                if (sender is BackupJobViewModel changedJob && changedJob.IsSelected)
                {
                    SelectedBackupJob = changedJob;
                }
                UpdateHasSelectedJobs();
            }
        }
        
        private void CreateBackupJob(object parameter)
        {
            var dialog = new BackupJobDialog();
            
            if (dialog.ShowDialog() == true)
            {
                var newJob = dialog.BackupJob;
                
                if (_backupJobRepository.AddBackupJob(newJob))
                {
                    var jobViewModel = new BackupJobViewModel(newJob);
                    jobViewModel.PropertyChanged += OnBackupJobViewModelPropertyChanged;
                    BackupJobs.Add(jobViewModel);
                    UpdateHasSelectedJobs();
                }
                else
                {
                    System.Windows.MessageBox.Show($"A job with the name '{newJob.JobName}' already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void EditBackupJob(object parameter)
        {
            if (SelectedBackupJob == null) return;
            
            var dialog = new BackupJobDialog(SelectedBackupJob.BackupJob);
            
            if (dialog.ShowDialog() == true)
            {
                var updatedJob = dialog.BackupJob;
                
                if (_backupJobRepository.UpdateBackupJob(updatedJob))
                {
                    // Update the view model
                    SelectedBackupJob.SourcePath = updatedJob.SourcePath;
                    SelectedBackupJob.TargetPath = updatedJob.TargetPath;
                    SelectedBackupJob.Description = updatedJob.Description;
                    SelectedBackupJob.Type = updatedJob.Type;
                }
                else
                {
                    System.Windows.MessageBox.Show($"Failed to update job '{updatedJob.JobName}'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void DeleteBackupJob(object parameter)
        {
            if (SelectedBackupJob == null) return;
            
            var result = System.Windows.MessageBox.Show($"Are you sure you want to delete the job '{SelectedBackupJob.JobName}'?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                if (_backupJobRepository.DeleteBackupJob(SelectedBackupJob.JobName))
                {
                    BackupJobs.Remove(SelectedBackupJob);
                    SelectedBackupJob = null;
                    UpdateHasSelectedJobs();
                }
                else
                {
                    System.Windows.MessageBox.Show($"Failed to delete job '{SelectedBackupJob.JobName}'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private async void ExecuteBackupJob(object parameter)
        {
            if (SelectedBackupJob == null) return;
            
            try
            {
                var job = SelectedBackupJob.BackupJob;
                await _backupService.StartBackupJobAsync(job);
                
                // Update the last backup time
                SelectedBackupJob.LastBackupTime = DateTime.Now;
                
                // Persist last backup date in repository
                UpdateLastBackupTime(job.JobName, DateTime.Now);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error executing backup job: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void ExecuteSelectedJobs(object parameter)
        {
            var selectedJobs = BackupJobs.Where(j => j.IsSelected).ToList();
            
            if (!selectedJobs.Any()) return;
            
            try
            {
                foreach (var jobViewModel in selectedJobs)
                {
                    var job = jobViewModel.BackupJob;
                    await _backupService.StartBackupJobAsync(job);
                    
                    // Update the last backup time
                    jobViewModel.LastBackupTime = DateTime.Now;
                    
                    // Persist last backup date in repository
                    UpdateLastBackupTime(job.JobName, DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error executing backup jobs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void OpenSettings(object parameter)
        {
            // Get the settings view model from the service provider
            var settingsViewModel = ((App)System.Windows.Application.Current).ServiceProvider.GetService(typeof(SettingsViewModel)) as SettingsViewModel;
            
            if (settingsViewModel != null)
            {
                var settingsWindow = new SettingsWindow(settingsViewModel);
                settingsWindow.ShowDialog();
            }
            else
            {
                System.Windows.MessageBox.Show("Could not create settings window.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void PauseResumeJob(object parameter)
        {
            Debug.WriteLine($"PauseResumeJob called with parameter: {parameter}");
            
            if (parameter is string jobName)
            {
                try
                {
                    // Find the job in the active jobs list
                    var activeJob = ActiveJobs.FirstOrDefault(j => j.JobName == jobName);
                    if (activeJob == null)
                    {
                        Debug.WriteLine($"Job {jobName} not found in active jobs");
                        return;
                    }
                    
                    // Use visual state to determine action
                    bool isPaused = activeJob.IsPaused;
                    Debug.WriteLine($"Job {jobName} is visually paused: {isPaused} (Status: {activeJob.Status})");
                    
                    bool result;
                    
                    if (isPaused)
                    {
                        // Job is visually paused, so we need to resume it
                        Debug.WriteLine($"Resuming job {jobName}");
                        result = await _backupService.ResumeJobAsync(jobName);
                        Debug.WriteLine($"Resume result: {result}");
                        
                        if (result)
                        {
                            // Update UI
                            activeJob.Status = JobStatus.Running;
                            activeJob.IsPaused = false;
                            activeJob.NotifyPropertyChanged(nameof(activeJob.Status));
                            activeJob.NotifyPropertyChanged(nameof(activeJob.IsPaused));
                        }
                    }
                    else
                    {
                        // Job is visually running, so we need to pause it
                        Debug.WriteLine($"Pausing job {jobName}");
                        result = await _backupService.PauseJobAsync(jobName);
                        Debug.WriteLine($"Pause result: {result}");
                        
                        if (result)
                        {
                            // Update UI
                            activeJob.Status = JobStatus.Paused;
                            activeJob.IsPaused = true;
                            activeJob.NotifyPropertyChanged(nameof(activeJob.Status));
                            activeJob.NotifyPropertyChanged(nameof(activeJob.IsPaused));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in PauseResumeJob: {ex.Message}");
                    System.Windows.MessageBox.Show($"Error pausing/resuming job: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void StopJob(object parameter)
        {
            Debug.WriteLine($"StopJob called for job: {parameter}");
            if (parameter is string jobName)
            {
                try
                {
                    bool result = _backupService.StopJob(jobName);
                    Debug.WriteLine($"StopJob result: {result}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in StopJob: {ex.Message}");
                    System.Windows.MessageBox.Show($"Error stopping job: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ToggleLanguage()
        {
            _translationService.ToggleLanguage();
            OnPropertyChanged(nameof(SelectedLanguage));
        }
        
        private void OnBackupJobStatusChanged(object sender, Models.BackupJobStatusEventArgs e)
        {
            // This event is raised from a background thread, so we need to dispatch to the UI thread
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Debug.WriteLine($"OnBackupJobStatusChanged: Status changed for job {e.JobName}: {e.Status}");
                
                // Check if we already have a view model for this job
                var jobViewModel = ActiveJobs.FirstOrDefault(j => j.JobName == e.JobName);
                
                // Normalize status to avoid case issues
                string normalizedStatus = NormalizeStatus(e.Status);
                
                if (jobViewModel == null)
                {
                    // Create a new view model for this job
                    jobViewModel = new ActiveBackupJobViewModel
                    {
                        JobName = e.JobName,
                        Status = normalizedStatus,
                        Progress = e.Progress,
                        CurrentFile = e.CurrentFile,
                        IsPaused = normalizedStatus == JobStatus.Paused
                    };
                    
                    ActiveJobs.Add(jobViewModel);
                    
                    // If this is the first active job, start monitoring
                    if (ActiveJobs.Count == 1)
                    {
                        StartBusinessSoftwareMonitoring();
                    }
                }
                else
                {
                    // Update all properties
                    jobViewModel.Progress = e.Progress;
                    jobViewModel.CurrentFile = e.CurrentFile;
                    
                    // Don't override pause state if we set it manually via Pause/Resume button, 
                    // except for final states
                    if (normalizedStatus == JobStatus.Completed || 
                        normalizedStatus == JobStatus.Canceled || 
                        normalizedStatus.StartsWith("Failed"))
                    {
                        jobViewModel.Status = normalizedStatus;
                        jobViewModel.IsPaused = false;
                    }
                }
                
                // Remove completed or canceled jobs after a delay
                if (normalizedStatus == JobStatus.Completed || normalizedStatus == JobStatus.Canceled || normalizedStatus.StartsWith("Failed"))
                {
                    Task.Delay(5000).ContinueWith(_ =>
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            ActiveJobs.Remove(jobViewModel);
                            
                            // If this was the last active job, stop monitoring
                            if (ActiveJobs.Count == 0)
                            {
                                StopBusinessSoftwareMonitoring();
                            }
                        });
                    });
                }
                
                // Update the last backup time for the corresponding job in the list
                var job = BackupJobs.FirstOrDefault(j => j.JobName == e.JobName);
                if (job != null && normalizedStatus == JobStatus.Completed)
                {
                    job.LastBackupTime = DateTime.Now;
                    
                    // Persist last backup date in repository
                    UpdateLastBackupTime(e.JobName, DateTime.Now);
                }
            });
        }

        private void UpdateLastBackupTime(string jobName, DateTime lastBackupTime)
        {
            try
            {
                // Get job from repository
                var job = _backupJobRepository.GetBackupJob(jobName);
                if (job != null)
                {
                    // Update last backup date
                    job.LastBackupTime = lastBackupTime;
                    
                    // Save changes to repository
                    _backupJobRepository.UpdateBackupJobLastBackupTime(jobName, lastBackupTime);
                    
                    Debug.WriteLine($"Last backup time for job {jobName} updated to {lastBackupTime} and persisted");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating last backup time: {ex.Message}");
            }
        }

        private string NormalizeStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
                return JobStatus.Running;
                
            // Convert to lowercase for comparison
            string lowerStatus = status.ToLowerInvariant();
            
            if (lowerStatus.Contains("pause"))
                return JobStatus.Paused;
            if (lowerStatus.Contains("en cours") || lowerStatus.Contains("running"))
                return JobStatus.Running;
            if (lowerStatus.Contains("complet"))
                return JobStatus.Completed;
            if (lowerStatus.Contains("cancel"))
                return JobStatus.Canceled;
            if (lowerStatus.Contains("stop"))
                return JobStatus.Stopping;
            if (lowerStatus.Contains("fail") || lowerStatus.Contains("error"))
                return "Failed: " + status;
                
            // If we don't recognize the status, return it as is
            return status;
        }

        private void OnBusinessSoftwareStatusChanged(object sender, bool isRunning)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusinessSoftwareRunning = isRunning;
                StatusMessage = isRunning 
                    ? "Business software is running. Backups are paused."
                    : string.Empty;
            });
        }

        private void OnTranslationServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // If the language changes, we need to update all the translation properties
            if (e.PropertyName == "CurrentLanguage" || e.PropertyName == "AllTranslations")
            {
                OnPropertyChanged(nameof(AppTitle));
                OnPropertyChanged(nameof(SelectedLanguage));
                OnPropertyChanged(nameof(SettingsButtonText));
                OnPropertyChanged(nameof(SelectAllText));
                OnPropertyChanged(nameof(BackupJobsHeader));
                OnPropertyChanged(nameof(NameLabel));
                OnPropertyChanged(nameof(SourcePathLabel));
                OnPropertyChanged(nameof(TargetPathLabel));
                OnPropertyChanged(nameof(TypeLabel));
                OnPropertyChanged(nameof(DescriptionLabel));
                OnPropertyChanged(nameof(LastBackupLabel));
                OnPropertyChanged(nameof(CreatedLabel));
                OnPropertyChanged(nameof(BackupJobDetailsHeader));
                OnPropertyChanged(nameof(JobStatusLabel));
                OnPropertyChanged(nameof(NotRunningText));
                OnPropertyChanged(nameof(ExecuteButtonText));
                OnPropertyChanged(nameof(EditButtonText));
                OnPropertyChanged(nameof(DeleteButtonText));
                OnPropertyChanged(nameof(ActiveJobsHeader));
                OnPropertyChanged(nameof(CreateButtonText));
                OnPropertyChanged(nameof(ExecuteSelectedJobsText));
                OnPropertyChanged(nameof(PauseButtonText));
                OnPropertyChanged(nameof(ResumeButtonText));
                OnPropertyChanged(nameof(StopButtonText));
                OnPropertyChanged(nameof(CurrentFileLabel));
                OnPropertyChanged(nameof(ProgressLabel));
            }
        }

        private void StartBusinessSoftwareMonitoring()
        {
            _businessSoftwareMonitor.Start();
        }

        private void StopBusinessSoftwareMonitoring()
        {
            _businessSoftwareMonitor.Stop();
        }
    }
}