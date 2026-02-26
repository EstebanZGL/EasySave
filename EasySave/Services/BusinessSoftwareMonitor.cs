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
    public class BusinessSoftwareMonitor : IDisposable
    {
        private readonly SettingsViewModel _settingsViewModel;
        private readonly object _backupService; // Can be IBackupService or ParallelBackupService
        private readonly IEncryptionLogger _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isMonitoring;
        private List<string> _pausedJobs = new List<string>();
        
        // Smart monitoring parameters
        private const int INITIAL_CHECK_INTERVAL = 1000;      // 1 second
        private const int NORMAL_CHECK_INTERVAL = 3000;       // 3 seconds
        private const int EXTENDED_CHECK_INTERVAL = 10000;    // 10 seconds
        private const int MAX_STABLE_CHECKS = 5;              // Number of stable checks before switching to extended interval
        private int _currentCheckInterval = INITIAL_CHECK_INTERVAL;
        private int _stableCheckCount = 0;
        private bool _lastStatus = false;
        
        public event EventHandler<bool> BusinessSoftwareStatusChanged;
        
        public BusinessSoftwareMonitor(SettingsViewModel settingsViewModel, ParallelBackupService backupService)
        {
            _settingsViewModel = settingsViewModel;
            _backupService = backupService;
            _logger = null;
        }
        
        public BusinessSoftwareMonitor(SettingsViewModel settingsViewModel, IBackupService backupService, IEncryptionLogger logger)
        {
            _settingsViewModel = settingsViewModel;
            _backupService = backupService;
            _logger = logger;
        }
        
        public void Start()
        {
            if (_isMonitoring)
            {
                return;
            }
            
            _isMonitoring = true;
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Reset monitoring parameters
            _currentCheckInterval = INITIAL_CHECK_INTERVAL;
            _stableCheckCount = 0;
            
            Task.Run(async () => await MonitorBusinessSoftwareAsync(_cancellationTokenSource.Token));
        }
        
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

        // Alias for Start method (for compatibility with BackupService)
        public void StartMonitoring()
        {
            Start();
        }

        // Alias for Stop method (for compatibility with BackupService)
        public void StopMonitoring()
        {
            Stop();
        }
        
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
                        // Status changed, reset interval and stability counter
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
                        // Status is stable, adjust check interval
                        if (isRunning == _lastStatus)
                        {
                            _stableCheckCount++;
                            
                            // If status is stable for several checks, increase interval
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
                            // Reset counter if status is different from last time
                            _stableCheckCount = 0;
                            _currentCheckInterval = INITIAL_CHECK_INTERVAL;
                        }
                    }
                    
                    _lastStatus = isRunning;
                    
                    // Wait according to current check interval
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
        
        private bool IsBusinessSoftwareRunning()
        {
            try
            {
                return _settingsViewModel.IsBusinessSoftwareRunning();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking business software status: {ex.Message}");
                return false;
            }
        }
        
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
        
        private void OnBusinessSoftwareStatusChanged(bool isRunning)
        {
            BusinessSoftwareStatusChanged?.Invoke(this, isRunning);
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }
    }
}