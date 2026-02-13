using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EasySave.ViewModels;
using EasyLog;

namespace EasySave.Services
{
    /// <summary>
    /// Service for monitoring business software and controlling backup operations
    /// </summary>
    public class BusinessSoftwareMonitor : IDisposable
    {
        private readonly SettingsViewModel _settings;
        private readonly BackupService _backupService;
        private readonly ILogger _logger;
        private System.Threading.Timer _monitorTimer;
        private bool _wasRunning;
        private const int CHECK_INTERVAL_MS = 1000; // Check every second

        public event EventHandler<bool> BusinessSoftwareStatusChanged;

        public BusinessSoftwareMonitor(SettingsViewModel settings, BackupService backupService, ILogger logger)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _wasRunning = _settings.IsBusinessSoftwareRunning();
        }

        /// <summary>
        /// Starts monitoring the business software
        /// </summary>
        public void StartMonitoring()
        {
            // Stop any existing timer
            StopMonitoring();

            // Create a new timer that checks the business software status
            _monitorTimer = new System.Threading.Timer(CheckBusinessSoftwareStatus, null, 0, CHECK_INTERVAL_MS);
            
            Debug.WriteLine($"Started monitoring business software: {_settings.BusinessSoftwareName}");
        }

        /// <summary>
        /// Stops monitoring the business software
        /// </summary>
        public void StopMonitoring()
        {
            if (_monitorTimer != null)
            {
                _monitorTimer.Dispose();
                _monitorTimer = null;
                Debug.WriteLine("Stopped monitoring business software");
            }
        }

        /// <summary>
        /// Checks if the business software is running and takes appropriate action
        /// </summary>
        private async void CheckBusinessSoftwareStatus(object state)
        {
            try
            {
                bool isRunning = _settings.IsBusinessSoftwareRunning();
                
                // If status changed, take action
                if (isRunning != _wasRunning)
                {
                    _wasRunning = isRunning;
                    
                    if (isRunning)
                    {
                        // Business software started, pause backup
                        Debug.WriteLine($"Business software {_settings.BusinessSoftwareName} started - pausing backup");
                        _backupService.PauseBackupJob();
                        await _logger.LogApplicationEventAsync(
                            "BackupPaused",
                            $"Backup paused because business software started: {_settings.BusinessSoftwareName}");
                    }
                    else
                    {
                        // Business software stopped, resume backup
                        Debug.WriteLine($"Business software {_settings.BusinessSoftwareName} stopped - resuming backup");
                        _backupService.ResumeBackupJob();
                        await _logger.LogApplicationEventAsync(
                            "BackupResumed",
                            $"Backup resumed because business software stopped: {_settings.BusinessSoftwareName}");
                    }
                    
                    // Notify subscribers about the status change
                    BusinessSoftwareStatusChanged?.Invoke(this, isRunning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in business software monitor: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if the business software is currently running
        /// </summary>
        /// <returns>True if the business software is running, false otherwise</returns>
        public bool IsBusinessSoftwareRunning()
        {
            return _settings.IsBusinessSoftwareRunning();
        }

        public void Dispose()
        {
            StopMonitoring();
        }
    }
}