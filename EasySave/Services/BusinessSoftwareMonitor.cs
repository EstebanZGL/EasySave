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
        private readonly object _backupService; // Can be BackupService or ParallelBackupService
        private readonly IEncryptionLogger _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isMonitoring;
        private List<string> _pausedJobs = new List<string>();
        
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
        /// Creates a new instance of the BusinessSoftwareMonitor with BackupService
        /// </summary>
        /// <param name="settingsViewModel">The settings view model</param>
        /// <param name="backupService">The backup service</param>
        /// <param name="logger">The logger</param>
        public BusinessSoftwareMonitor(SettingsViewModel settingsViewModel, BackupService backupService, IEncryptionLogger logger)
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
        /// Monitors for business software in a loop
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
                        wasRunning = isRunning;
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
                    
                    // Check every second
                    await Task.Delay(1000, cancellationToken);
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
            // Use the IsBusinessSoftwareRunning method from SettingsViewModel
            return _settingsViewModel.IsBusinessSoftwareRunning();
        }
        
        /// <summary>
        /// Pauses all active backup jobs
        /// </summary>
        private async Task PauseAllActiveJobsAsync()
        {
            _pausedJobs.Clear();
            
            if (_backupService is ParallelBackupService parallelService)
            {
                foreach (var job in parallelService.GetActiveJobs())
                {
                    if (job.Status != "Paused" && job.Status != "Completed" && 
                        job.Status != "Canceled" && !job.Status.StartsWith("Failed"))
                    {
                        await parallelService.PauseJobAsync(job.JobName);
                        _pausedJobs.Add(job.JobName);
                    }
                }
            }
            else if (_backupService is BackupService service)
            {
                service.PauseBackupJob();
                // No need to track job names for the old BackupService
            }
        }
        
        /// <summary>
        /// Resumes all previously paused backup jobs
        /// </summary>
        private async Task ResumeAllPausedJobsAsync()
        {
            if (_backupService is ParallelBackupService parallelService)
            {
                foreach (var jobName in _pausedJobs)
                {
                    await parallelService.ResumeJobAsync(jobName);
                }
            }
            else if (_backupService is BackupService service)
            {
                service.ResumeBackupJob();
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