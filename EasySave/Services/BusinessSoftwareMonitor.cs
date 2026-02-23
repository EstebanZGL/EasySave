using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using EasySave.ViewModels;
using EasyLog;

namespace EasySave.Services
{
    /// <summary>
    /// Service for monitoring business software that should pause backups when running
    /// </summary>
    public class BusinessSoftwareMonitor : IDisposable
    {
        private readonly SettingsViewModel _settingsViewModel;
        private readonly object _backupService; // Can be IBackupService or ParallelBackupService
        private readonly IEncryptionLogger _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isMonitoring;
        private List<string> _pausedJobs = new List<string>();
        
        // Paramètres de surveillance intelligente
        private const int INITIAL_CHECK_INTERVAL = 1000;      // Intervalle initial (1 seconde)
        private const int NORMAL_CHECK_INTERVAL = 3000;       // Intervalle normal (3 secondes)
        private const int EXTENDED_CHECK_INTERVAL = 10000;    // Intervalle étendu (10 secondes)
        private const int MAX_STABLE_CHECKS = 5;              // Nombre de vérifications stables avant de passer à l'intervalle étendu
        private int _currentCheckInterval = INITIAL_CHECK_INTERVAL;
        private int _stableCheckCount = 0;
        private bool _lastStatus = false;
        
        /// <summary>
        /// Event raised when business software status changes
        /// </summary>
        public event EventHandler<bool> BusinessSoftwareStatusChanged;
        
        /// <summary>
        /// Creates a new instance of the BusinessSoftwareMonitor with ParallelBackupService
        /// </summary>
        /// <param name="settingsViewModel">The settings view model</param>
        /// <param name="backupService">The parallel backup service</param>
        public BusinessSoftwareMonitor(SettingsViewModel settingsViewModel, ParallelBackupService backupService)
        {
            _settingsViewModel = settingsViewModel;
            _backupService = backupService;
            _logger = null;
        }
        
        /// <summary>
        /// Creates a new instance of the BusinessSoftwareMonitor with IBackupService
        /// </summary>
        /// <param name="settingsViewModel">The settings view model</param>
        /// <param name="backupService">The backup service</param>
        /// <param name="logger">The logger</param>
        public BusinessSoftwareMonitor(SettingsViewModel settingsViewModel, IBackupService backupService, IEncryptionLogger logger)
        {
            _settingsViewModel = settingsViewModel;
            _backupService = backupService;
            _logger = logger;
        }
        
        /// <summary>
        /// Starts monitoring for business software
        /// </summary>
        public void Start()
        {
            if (_isMonitoring)
            {
                return;
            }
            
            _isMonitoring = true;
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Réinitialiser les paramètres de surveillance
            _currentCheckInterval = INITIAL_CHECK_INTERVAL;
            _stableCheckCount = 0;
            
            Task.Run(async () => await MonitorBusinessSoftwareAsync(_cancellationTokenSource.Token));
        }
        
        /// <summary>
        /// Stops monitoring for business software
        /// </summary>
        public void Stop()
        {
            if (!_isMonitoring)
            {
                return;
            }
            
            _isMonitoring = false;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        /// <summary>
        /// Alias for Start method (for compatibility with BackupService)
        /// </summary>
        public void StartMonitoring()
        {
            Start();
        }

        /// <summary>
        /// Alias for Stop method (for compatibility with BackupService)
        /// </summary>
        public void StopMonitoring()
        {
            Stop();
        }
        
        /// <summary>
        /// Monitors for business software in a loop with adaptive checking intervals
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation</param>
        private async Task MonitorBusinessSoftwareAsync(CancellationToken cancellationToken)
        {
            bool wasRunning = false;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    bool isRunning = IsBusinessSoftwareRunning();
                    
                    if (isRunning != wasRunning)
                    {
                        // Le statut a changé, réinitialiser l'intervalle et le compteur de stabilité
                        _currentCheckInterval = INITIAL_CHECK_INTERVAL;
                        _stableCheckCount = 0;
                        wasRunning = isRunning;
                        
                        Debug.WriteLine($"Business software status changed to: {(isRunning ? "Running" : "Not running")}");
                        OnBusinessSoftwareStatusChanged(isRunning);
                        
                        if (isRunning)
                        {
                            // Business software started running, pause all active jobs
                            await PauseAllActiveJobsAsync();
                        }
                        else
                        {
                            // Business software stopped running, resume all paused jobs
                            await ResumeAllPausedJobsAsync();
                        }
                    }
                    else
                    {
                        // Le statut est stable, ajuster l'intervalle de vérification
                        if (isRunning == _lastStatus)
                        {
                            _stableCheckCount++;
                            
                            // Si le statut est stable depuis plusieurs vérifications, augmenter l'intervalle
                            if (_stableCheckCount >= MAX_STABLE_CHECKS)
                            {
                                _currentCheckInterval = EXTENDED_CHECK_INTERVAL;
                            }
                            else if (_stableCheckCount >= 2)
                            {
                                _currentCheckInterval = NORMAL_CHECK_INTERVAL;
                            }
                        }
                        else
                        {
                            // Réinitialiser le compteur si le statut est différent de la dernière fois
                            _stableCheckCount = 0;
                            _currentCheckInterval = INITIAL_CHECK_INTERVAL;
                        }
                    }
                    
                    _lastStatus = isRunning;
                    
                    // Attendre selon l'intervalle de vérification actuel
                    await Task.Delay(_currentCheckInterval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Monitoring was canceled
                    break;
                }
                catch (Exception ex)
                {
                    // Log the error but continue monitoring
                    Debug.WriteLine($"Error monitoring business software: {ex.Message}");
                    await Task.Delay(5000, cancellationToken); // Longer delay after error
                }
            }
        }
        
        /// <summary>
        /// Checks if any configured business software is running
        /// </summary>
        /// <returns>True if business software is running, false otherwise</returns>
        private bool IsBusinessSoftwareRunning()
        {
            try
            {
                // Use the IsBusinessSoftwareRunning method from SettingsViewModel
                return _settingsViewModel.IsBusinessSoftwareRunning();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking business software status: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Pauses all active backup jobs
        /// </summary>
        private async Task PauseAllActiveJobsAsync()
        {
            Debug.WriteLine("Pausing all active backup jobs due to business software running");
            _pausedJobs.Clear();
            
            try
            {
                if (_backupService is ParallelBackupService parallelService)
                {
                    foreach (var job in parallelService.GetActiveJobs())
                    {
                        if (job.Status != "Paused" && job.Status != "Completed" && 
                            job.Status != "Canceled" && !job.Status.StartsWith("Failed"))
                        {
                            await parallelService.PauseJobAsync(job.JobName);
                            _pausedJobs.Add(job.JobName);
                            Debug.WriteLine($"Paused job: {job.JobName}");
                        }
                    }
                }
                else if (_backupService is IBackupService service)
                {
                    service.PauseBackupJob();
                    Debug.WriteLine("Paused backup job in BackupService");
                    // No need to track job names for the old BackupService
                }
                
                // Log the event if logger is available
                if (_logger != null)
                {
                    await _logger.LogApplicationEventAsync(
                        "BusinessSoftwareStarted",
                        $"Backup paused because business software started: {_settingsViewModel.BusinessSoftwareName}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error pausing jobs: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Resumes all previously paused backup jobs
        /// </summary>
        private async Task ResumeAllPausedJobsAsync()
        {
            Debug.WriteLine("Resuming all paused backup jobs as business software is no longer running");
            
            try
            {
                if (_backupService is ParallelBackupService parallelService)
                {
                    foreach (var jobName in _pausedJobs)
                    {
                        await parallelService.ResumeJobAsync(jobName);
                        Debug.WriteLine($"Resumed job: {jobName}");
                    }
                }
                else if (_backupService is IBackupService service)
                {
                    service.ResumeBackupJob();
                    Debug.WriteLine("Resumed backup job in BackupService");
                }
                
                // Log the event if logger is available
                if (_logger != null)
                {
                    await _logger.LogApplicationEventAsync(
                        "BusinessSoftwareStopped",
                        $"Backup resumed because business software stopped: {_settingsViewModel.BusinessSoftwareName}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error resuming jobs: {ex.Message}");
            }
            
            _pausedJobs.Clear();
        }
        
        /// <summary>
        /// Raises the BusinessSoftwareStatusChanged event
        /// </summary>
        /// <param name="isRunning">Whether business software is running</param>
        private void OnBusinessSoftwareStatusChanged(bool isRunning)
        {
            BusinessSoftwareStatusChanged?.Invoke(this, isRunning);
        }

        /// <summary>
        /// Disposes resources used by the monitor
        /// </summary>
        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }
    }
}