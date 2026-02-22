using EasyLog;
using EasySave.Models;
using EasySave.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Timers;

namespace EasySave.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _businessSoftwareName;
        private List<string> _encryptionExtensions;
        private string _cryptoSoftPath;
        private int _maxParallelJobs;
        private string _logFormat;
        private long _largeFileThreshold;
        private List<string> _priorityExtensions;
        private LogCentralizationSettings _logCentralizationSettings;
        private readonly TranslationService _translationService;

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsViewModel(TranslationService translationService = null)
        {
            _translationService = translationService;
            
            // Default values
            _businessSoftwareName = "calc.exe";  // Default to Calculator for demo
            _encryptionExtensions = new List<string> { ".txt", ".doc", ".pdf" };
            _priorityExtensions = new List<string> { ".exe", ".dll", ".sys" };
            _cryptoSoftPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CryptoSoft.exe");
            _maxParallelJobs = 5;
            _logFormat = "JSON"; // Default log format
            _largeFileThreshold = 1024 * 1024; // 1MB default
            _logCentralizationSettings = new LogCentralizationSettings();
            
            LoadSettings();
            LoadLogFormat(); // Load log format from separate file
            
            // Debug: Print the current business software name
            Debug.WriteLine($"Business software name set to: {_businessSoftwareName}");
            
            // Subscribe to translation changes if service is available
            if (_translationService != null)
            {
                _translationService.PropertyChanged += OnTranslationServicePropertyChanged;
            }
        }

        public string BusinessSoftwareName
        {
            get => _businessSoftwareName;
            set
            {
                if (_businessSoftwareName != value)
                {
                    _businessSoftwareName = value;
                    OnPropertyChanged();
                    SaveSettings();
                    
                    // Debug: Print when business software name changes
                    Debug.WriteLine($"Business software name changed to: {_businessSoftwareName}");
                }
            }
        }

        public List<string> EncryptionExtensions
        {
            get => _encryptionExtensions;
            set
            {
                if (_encryptionExtensions != value)
                {
                    _encryptionExtensions = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ExtensionsToEncrypt)); // Update the string[] property too
                    SaveSettings();
                }
            }
        }

        // Property needed for compatibility with CryptoServiceTests
        public string[] ExtensionsToEncrypt
        {
            get => _encryptionExtensions?.ToArray();
            set
            {
                if (value != null)
                {
                    EncryptionExtensions = new List<string>(value);
                }
                else
                {
                    EncryptionExtensions = new List<string>();
                }
            }
        }

        public List<string> PriorityExtensions
        {
            get => _priorityExtensions;
            set
            {
                if (_priorityExtensions != value)
                {
                    _priorityExtensions = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        public string CryptoSoftPath
        {
            get => _cryptoSoftPath;
            set
            {
                if (_cryptoSoftPath != value)
                {
                    _cryptoSoftPath = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        public int MaxParallelJobs
        {
            get => _maxParallelJobs;
            set
            {
                if (_maxParallelJobs != value)
                {
                    _maxParallelJobs = Math.Max(1, value); // Ensure at least 1 job
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        public long LargeFileThreshold
        {
            get => _largeFileThreshold;
            set
            {
                if (_largeFileThreshold != value)
                {
                    _largeFileThreshold = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LargeFileThresholdMB));
                    SaveSettings();
                }
            }
        }

        // Nouvelle propriété pour afficher et définir la taille en MB uniquement
        public int LargeFileThresholdMB
        {
            get => (int)(_largeFileThreshold / (1024 * 1024));
            set
            {
                // Convertir MB en bytes
                long newThreshold = value * 1024L * 1024L;
                if (_largeFileThreshold != newThreshold)
                {
                    _largeFileThreshold = newThreshold;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LargeFileThresholdDisplay));
                    SaveSettings();
                }
            }
        }

        // Propriété pour l'affichage uniquement (pour la rétrocompatibilité)
        public string LargeFileThresholdDisplay
        {
            get => $"{LargeFileThresholdMB} MB";
            set
            {
                // Extraire uniquement la partie numérique
                string numericPart = new string(value.TakeWhile(c => char.IsDigit(c)).ToArray());
                
                if (int.TryParse(numericPart, out int mbValue))
                {
                    LargeFileThresholdMB = mbValue;
                }
            }
        }

        public string LogFormat
        {
            get => _logFormat;
            set
            {
                if (_logFormat != value)
                {
                    _logFormat = value;
                    OnPropertyChanged();
                    SaveLogFormat();
                }
            }
        }

        public LogCentralizationSettings LogCentralization
        {
            get => _logCentralizationSettings;
            set
            {
                if (_logCentralizationSettings != value)
                {
                    _logCentralizationSettings = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        public string EncryptionExtensionsString
        {
            get => string.Join(";", _encryptionExtensions);
            set
            {
                var extensions = value
                    .Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim().ToLowerInvariant())
                    .Select(e => e.StartsWith(".") ? e : "." + e)
                    .Distinct()
                    .ToList();

                EncryptionExtensions = extensions;
                OnPropertyChanged();
            }
        }

        public string PriorityExtensionsString
        {
            get => string.Join(";", _priorityExtensions);
            set
            {
                var extensions = value
                    .Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim().ToLowerInvariant())
                    .Select(e => e.StartsWith(".") ? e : "." + e)
                    .Distinct()
                    .ToList();

                PriorityExtensions = extensions;
                OnPropertyChanged();
            }
        }

        // Propriétés de traduction
        public string WindowTitle => GetTranslation("menu_settings");
        public string GeneralSettingsHeader => GetTranslation("general_settings");
        public string BusinessSoftwareLabel => GetTranslation("business_software");
        public string CryptoSoftPathLabel => GetTranslation("cryptosoft_path");
        public string LogFormatLabel => GetTranslation("log_format_setting");
        public string RestartRequiredText => GetTranslation("restart_required");
        public string ParallelBackupSettingsHeader => GetTranslation("parallel_backup_settings");
        public string MaxParallelJobsLabel => GetTranslation("max_parallel_jobs");
        public string LargeFileThresholdLabel => GetTranslation("large_file_threshold");
        public string PriorityExtensionsLabel => GetTranslation("priority_extensions");
        public string EncryptionSettingsHeader => GetTranslation("encryption_settings");
        public string EncryptExtensionsLabel => GetTranslation("encrypt_extensions");
        public string LogCentralizationHeader => GetTranslation("log_centralization_settings");
        public string EnableCentralizationLabel => GetTranslation("enable_centralization");
        public string LogServerUrlLabel => GetTranslation("log_server_url");
        public string LogDestinationLabel => GetTranslation("log_destination");
        public string LocalOnlyText => GetTranslation("local_only");
        public string RemoteOnlyText => GetTranslation("remote_only");
        public string BothLocalRemoteText => GetTranslation("both_local_remote");
        public string NotesLabel => GetTranslation("settings_notes");
        public string BusinessSoftwareNote => GetTranslation("settings_business_note");
        public string MaxParallelJobsNote => GetTranslation("max_parallel_jobs_note");
        public string LargeFileThresholdNote => GetTranslation("large_file_threshold_note");
        public string PriorityExtensionsNote => GetTranslation("priority_extensions_note");
        public string EncryptExtensionsNote => GetTranslation("settings_encrypt_note");
        public string LogCentralizationNote => GetTranslation("log_centralization_note");
        public string ChangesNote => GetTranslation("settings_changes_note");
        public string CloseButtonText => GetTranslation("close");
        public string BrowseButtonText => GetTranslation("browse");

        /// <summary>
        /// Checks if the configured business software is currently running using multiple detection methods
        /// </summary>
        /// <returns>True if the business software is running, false otherwise</returns>
        public bool IsBusinessSoftwareRunning()
        {
            if (string.IsNullOrWhiteSpace(BusinessSoftwareName))
            {
                Debug.WriteLine("Business software name is empty, returning false");
                return false;
            }

            try
            {
                string processNameToFind = Path.GetFileNameWithoutExtension(BusinessSoftwareName).ToLowerInvariant();
                Debug.WriteLine($"Looking for business software: {processNameToFind}");

                // Method 1: Simple process name check (most reliable but limited)
                try
                {
                    var processesByName = Process.GetProcessesByName(processNameToFind);
                    if (processesByName.Length > 0)
                    {
                        Debug.WriteLine($"Found {processesByName.Length} processes with name '{processNameToFind}'");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in method 1: {ex.Message}");
                }

                // Method 2: Check for Calculator specifically (special case handling)
                if (processNameToFind.Equals("calc", StringComparison.OrdinalIgnoreCase) || 
                    processNameToFind.Equals("calculator", StringComparison.OrdinalIgnoreCase))
                {
                    // Modern Windows Calculator (Windows 10/11)
                    var calculatorProcesses = Process.GetProcessesByName("CalculatorApp");
                    if (calculatorProcesses.Length > 0)
                    {
                        Debug.WriteLine("Found modern Calculator (CalculatorApp)");
                        return true;
                    }

                    // Check if Calculator is hosted in ApplicationFrameHost
                    try
                    {
                        foreach (var process in Process.GetProcessesByName("ApplicationFrameHost"))
                        {
                            try
                            {
                                // This is a heuristic - if Calculator is open, its window title often contains "Calculator"
                                if (process.MainWindowTitle.Contains("Calculator", StringComparison.OrdinalIgnoreCase) ||
                                    process.MainWindowTitle.Contains("Calculatrice", StringComparison.OrdinalIgnoreCase))
                                {
                                    Debug.WriteLine("Found Calculator running in ApplicationFrameHost");
                                    return true;
                                }
                            }
                            catch
                            {
                                // Ignore access errors
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error checking ApplicationFrameHost: {ex.Message}");
                    }
                }

                // Method 3: Check all processes and their window titles
                try
                {
                    foreach (var process in Process.GetProcesses())
                    {
                        try
                        {
                            // Check if the process name contains our target
                            if (process.ProcessName.ToLowerInvariant().Contains(processNameToFind))
                            {
                                Debug.WriteLine($"Found process with matching name: {process.ProcessName}");
                                return true;
                            }

                            // Check window title (can be useful for hosted apps like UWP apps)
                            if (!string.IsNullOrEmpty(process.MainWindowTitle) && 
                                process.MainWindowTitle.ToLowerInvariant().Contains(processNameToFind))
                            {
                                Debug.WriteLine($"Found process with matching window title: {process.MainWindowTitle}");
                                return true;
                            }
                        }
                        catch
                        {
                            // Ignore access errors for individual processes
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error checking all processes: {ex.Message}");
                }

                // Method 4: Use PowerShell as a last resort
                try
                {
                    using (Process process = new Process())
                    {
                        process.StartInfo.FileName = "powershell.exe";
                        process.StartInfo.Arguments = $"-Command \"Get-Process | Where-Object {{ $_.ProcessName -like '*{processNameToFind}*' }} | Measure-Object | Select-Object -ExpandProperty Count\"";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.CreateNoWindow = true;
                        
                        process.Start();
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        
                        if (int.TryParse(output.Trim(), out int count) && count > 0)
                        {
                            Debug.WriteLine($"Found {count} matching processes via PowerShell");
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in PowerShell method: {ex.Message}");
                }

                Debug.WriteLine("Business software is NOT running (checked all methods)");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in IsBusinessSoftwareRunning: {ex.Message}");
                return false; // In case of error, assume it's not running
            }
        }

        public bool ShouldEncryptFile(string filePath)
        {
            if (EncryptionExtensions == null || EncryptionExtensions.Count == 0)
                return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return EncryptionExtensions.Any(e => string.Equals(e, extension, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsPriorityFile(string filePath)
        {
            if (PriorityExtensions == null || PriorityExtensions.Count == 0)
                return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return PriorityExtensions.Any(e => string.Equals(e, extension, StringComparison.OrdinalIgnoreCase));
        }

        private void LoadSettings()
        {
            try
            {
                string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                Debug.WriteLine($"Loading settings from: {settingsPath}");
                
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    var settings = JsonSerializer.Deserialize<SettingsData>(json);
                    
                    if (settings != null)
                    {
                        BusinessSoftwareName = settings.BusinessSoftwareName;
                        EncryptionExtensions = settings.EncryptionExtensions ?? new List<string> { ".txt", ".doc", ".pdf" };
                        PriorityExtensions = settings.PriorityExtensions ?? new List<string> { ".exe", ".dll", ".sys" };
                        CryptoSoftPath = settings.CryptoSoftPath;
                        MaxParallelJobs = settings.MaxParallelJobs;
                        LargeFileThreshold = settings.LargeFileThreshold > 0 ? settings.LargeFileThreshold : 1024 * 1024;
                        
                        if (settings.LogCentralization != null)
                        {
                            _logCentralizationSettings.IsEnabled = settings.LogCentralization.IsEnabled;
                            _logCentralizationSettings.LogDestination = settings.LogCentralization.LogDestination;
                            _logCentralizationSettings.ServerUrl = settings.LogCentralization.ServerUrl;
                        }
                        
                        Debug.WriteLine($"Loaded settings: BusinessSoftwareName={BusinessSoftwareName}");
                    }
                    else
                    {
                        Debug.WriteLine("Settings were null after deserialization");
                    }
                }
                else
                {
                    Debug.WriteLine("Settings file does not exist, using defaults");
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with default settings
                Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                var settings = new SettingsData
                {
                    BusinessSoftwareName = BusinessSoftwareName,
                    EncryptionExtensions = EncryptionExtensions,
                    PriorityExtensions = PriorityExtensions,
                    CryptoSoftPath = CryptoSoftPath,
                    MaxParallelJobs = MaxParallelJobs,
                    LargeFileThreshold = LargeFileThreshold,
                    LogCentralization = new LogCentralizationSettings
                    {
                        IsEnabled = _logCentralizationSettings.IsEnabled,
                        LogDestination = _logCentralizationSettings.LogDestination,
                        ServerUrl = _logCentralizationSettings.ServerUrl
                    }
                };
                
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsPath, json);
                
                Debug.WriteLine($"Settings saved: BusinessSoftwareName={BusinessSoftwareName}");
            }
            catch (Exception ex)
            {
                // Log error
                Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private void LoadLogFormat()
        {
            try
            {
                string logFormatPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logformat.txt");
                if (File.Exists(logFormatPath))
                {
                    string format = File.ReadAllText(logFormatPath).Trim();
                    if (format == "XML" || format == "JSON")
                    {
                        _logFormat = format;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with default format
                Debug.WriteLine($"Error loading log format: {ex.Message}");
            }
        }

        private void SaveLogFormat()
        {
            try
            {
                string logFormatPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logformat.txt");
                File.WriteAllText(logFormatPath, _logFormat);
            }
            catch (Exception ex)
            {
                // Log error
                Debug.WriteLine($"Error saving log format: {ex.Message}");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnTranslationServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentLanguage" || e.PropertyName == "AllTranslations")
            {
                // Mettre à jour toutes les propriétés de traduction
                OnPropertyChanged(nameof(WindowTitle));
                OnPropertyChanged(nameof(GeneralSettingsHeader));
                OnPropertyChanged(nameof(BusinessSoftwareLabel));
                OnPropertyChanged(nameof(CryptoSoftPathLabel));
                OnPropertyChanged(nameof(LogFormatLabel));
                OnPropertyChanged(nameof(RestartRequiredText));
                OnPropertyChanged(nameof(ParallelBackupSettingsHeader));
                OnPropertyChanged(nameof(MaxParallelJobsLabel));
                OnPropertyChanged(nameof(LargeFileThresholdLabel));
                OnPropertyChanged(nameof(PriorityExtensionsLabel));
                OnPropertyChanged(nameof(EncryptionSettingsHeader));
                OnPropertyChanged(nameof(EncryptExtensionsLabel));
                OnPropertyChanged(nameof(LogCentralizationHeader));
                OnPropertyChanged(nameof(EnableCentralizationLabel));
                OnPropertyChanged(nameof(LogServerUrlLabel));
                OnPropertyChanged(nameof(LogDestinationLabel));
                OnPropertyChanged(nameof(LocalOnlyText));
                OnPropertyChanged(nameof(RemoteOnlyText));
                OnPropertyChanged(nameof(BothLocalRemoteText));
                OnPropertyChanged(nameof(NotesLabel));
                OnPropertyChanged(nameof(BusinessSoftwareNote));
                OnPropertyChanged(nameof(MaxParallelJobsNote));
                OnPropertyChanged(nameof(LargeFileThresholdNote));
                OnPropertyChanged(nameof(PriorityExtensionsNote));
                OnPropertyChanged(nameof(EncryptExtensionsNote));
                OnPropertyChanged(nameof(LogCentralizationNote));
                OnPropertyChanged(nameof(ChangesNote));
                OnPropertyChanged(nameof(CloseButtonText));
                OnPropertyChanged(nameof(BrowseButtonText));
            }
        }

        private string GetTranslation(string key)
        {
            return _translationService?.GetTranslation(key) ?? key;
        }

        private class SettingsData
        {
            public string BusinessSoftwareName { get; set; }
            public List<string> EncryptionExtensions { get; set; }
            public List<string> PriorityExtensions { get; set; }
            public string CryptoSoftPath { get; set; }
            public int MaxParallelJobs { get; set; }
            public long LargeFileThreshold { get; set; }
            public LogCentralizationSettings LogCentralization { get; set; }
        }
    }
}