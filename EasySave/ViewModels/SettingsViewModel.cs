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

namespace EasySave.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private static readonly object SettingsFileLock = new object();
        private static readonly object LogFormatFileLock = new object();

        // Services
        private readonly TranslationService _translationService;
        private readonly string _settingsFilePath;
        private readonly CryptoPasswordService _cryptoPasswordService;
        
        // Private fields
        private string _language = "en";
        private string _logFormat = "json";
        private string _businessSoftwareName = "";
        private string _cryptoSoftPath = "";
        private List<string> _encryptionExtensions = new List<string>();
        private List<string> _priorityExtensions = new List<string>();
        private long _largeFileThreshold = 1048576; // 1MB default
        private int _maxParallelJobs = 5;
        private string _encryptionKey = "";
        private LogCentralizationSettings _logCentralizationSettings = new LogCentralizationSettings();

        public event PropertyChangedEventHandler? PropertyChanged;

        // Constructor without parameters for testing
        public SettingsViewModel() : this(null)
        {
        }

        public SettingsViewModel(TranslationService translationService = null)
        {
            _translationService = translationService;
            _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            _cryptoPasswordService = new CryptoPasswordService();
            
            // Default values
            _businessSoftwareName = "calc.exe";
            _encryptionExtensions = new List<string> { ".txt", ".doc", ".pdf" };
            _priorityExtensions = new List<string> { ".exe", ".dll", ".sys" };
            _cryptoSoftPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CryptoSoft.exe");
            _maxParallelJobs = 5;
            _logFormat = "JSON";
            _largeFileThreshold = 1048576; // 1MB
            
            // Load settings
            LoadSettings();
            LoadLogFormat();
            
            // CRITICAL: Read key from shared file at startup
            _encryptionKey = _cryptoPasswordService.GetPassword();
            
            // Subscribe to translations
            if (_translationService != null)
            {
                _translationService.PropertyChanged += OnTranslationServicePropertyChanged;
            }
        }

        // --- Public Properties ---

        public string EncryptionKey
        {
            get => _encryptionKey;
            set
            {
                if (_encryptionKey != value)
                {
                    _encryptionKey = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CryptoPassword)); // Notify alias for view
                    
                    // CRITICAL: Write immediately to shared file
                    bool success = _cryptoPasswordService.SetPassword(value);
                    if (!success)
                    {
                        Debug.WriteLine("Critical error: Unable to save shared key!");
                    }
                }
            }
        }

        // --- Compatibility members for SettingsWindow.xaml.cs ---

        public string CryptoPassword
        {
            get => EncryptionKey;
            set => EncryptionKey = value;
        }

        public bool ChangeCryptoPassword(string newPassword)
        {
            EncryptionKey = newPassword;
            return true;
        }

        // Virtual property to allow mocking in CryptoServiceTests
        public virtual string[] ExtensionsToEncrypt
        {
            get => _encryptionExtensions?.ToArray() ?? Array.Empty<string>();
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

        public string Language
        {
            get => _language;
            set { if (_language != value) { _language = value; OnPropertyChanged(); SaveSettings(); } }
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

        public string BusinessSoftwareName
        {
            get => _businessSoftwareName;
            set { if (_businessSoftwareName != value) { _businessSoftwareName = value; OnPropertyChanged(); SaveSettings(); } }
        }

        public string CryptoSoftPath
        {
            get => _cryptoSoftPath;
            set { if (_cryptoSoftPath != value) { _cryptoSoftPath = value; OnPropertyChanged(); SaveSettings(); } }
        }

        public int MaxParallelJobs
        {
            get => _maxParallelJobs;
            set
            {
                if (_maxParallelJobs != value)
                {
                    _maxParallelJobs = Math.Max(1, value);
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        public List<string> EncryptionExtensions
        {
            get => _encryptionExtensions;
            set { if (_encryptionExtensions != value) { _encryptionExtensions = value; OnPropertyChanged(); SaveSettings(); } }
        }
        
        public List<string> PriorityExtensions
        {
            get => _priorityExtensions;
            set { if (_priorityExtensions != value) { _priorityExtensions = value; OnPropertyChanged(); SaveSettings(); } }
        }

        public long LargeFileThreshold
        {
            get => _largeFileThreshold;
            set
            {
                if (_largeFileThreshold != value)
                {
                    _largeFileThreshold = Math.Max(1024, value);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LargeFileThresholdMB));
                    SaveSettings();
                }
            }
        }

        // Property to display and set size in MB only
        public int LargeFileThresholdMB
        {
            get => (int)(_largeFileThreshold / (1024 * 1024));
            set
            {
                // Convert MB to bytes
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

        // Display property (for backward compatibility)
        public string LargeFileThresholdDisplay
        {
            get => $"{LargeFileThresholdMB} MB";
            set
            {
                // Extract only numeric part
                string numericPart = new string(value.TakeWhile(c => char.IsDigit(c)).ToArray());
                
                if (int.TryParse(numericPart, out int mbValue))
                {
                    LargeFileThresholdMB = mbValue;
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

        // --- Translation Properties ---

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
        public string CryptoPasswordLabel => GetTranslation("encryption_key");
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
        public string CryptoPasswordNote => GetTranslation("settings_key_note");
        public string LogCentralizationNote => GetTranslation("log_centralization_note");
        public string ChangesNote => GetTranslation("settings_changes_note");
        public string CloseButtonText => GetTranslation("close");
        public string BrowseButtonText => GetTranslation("browse");
        public string LanguageLabel => GetTranslation("language");
        public string EnglishLanguageText => GetTranslation("language_english");
        public string FrenchLanguageText => GetTranslation("language_french");

        public string SelectedLanguageCode
        {
            get => _translationService?.CurrentLanguage ?? "en";
            set
            {
                if (_translationService == null || string.IsNullOrWhiteSpace(value))
                    return;

                string normalized = value.Trim().ToLowerInvariant();
                if (normalized != "en" && normalized != "fr")
                    return;

                if (_translationService.CurrentLanguage != normalized)
                {
                    _translationService.SetLanguage(normalized);
                    OnPropertyChanged();
                }
            }
        }

        public virtual bool IsBusinessSoftwareRunning()
        {
            if (string.IsNullOrEmpty(_businessSoftwareName))
                return false;

            try
            {
                string processNameToFind = Path.GetFileNameWithoutExtension(BusinessSoftwareName).ToLowerInvariant();
                Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_businessSoftwareName));
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public virtual bool ShouldEncryptFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || _encryptionExtensions == null || _encryptionExtensions.Count == 0)
                return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
                return false;
                
            return EncryptionExtensions.Any(e => string.Equals(e, extension, StringComparison.OrdinalIgnoreCase));
        }

        public virtual bool IsPriorityFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || _priorityExtensions == null || _priorityExtensions.Count == 0)
                return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
                return false;
                
            return PriorityExtensions.Any(e => string.Equals(e, extension, StringComparison.OrdinalIgnoreCase));
        }

        protected virtual void LoadSettings()
        {
            lock (SettingsFileLock)
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return;
                }

                try
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<SettingsModel>(json);
                    if (settings != null)
                    {
                        _language = settings.Language ?? "en";
                        _logFormat = settings.LogFormat ?? "json";
                        _businessSoftwareName = settings.BusinessSoftwareName ?? "";
                        _cryptoSoftPath = settings.CryptoSoftPath ?? _cryptoSoftPath;
                        _encryptionExtensions = settings.EncryptionExtensions ?? new List<string>();
                        _priorityExtensions = settings.PriorityExtensions ?? new List<string>();
                        _largeFileThreshold = settings.LargeFileThreshold > 0 ? settings.LargeFileThreshold : 1048576;
                        _maxParallelJobs = settings.MaxParallelJobs > 0 ? settings.MaxParallelJobs : 5;
                        
                        if (settings.LogCentralization != null)
                        {
                            _logCentralizationSettings = settings.LogCentralization;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading settings: {ex.Message}");
                }
            }
        }

        protected virtual void SaveSettings()
        {
            try
            {
                var settings = new SettingsModel
                {
                    Language = _language,
                    LogFormat = _logFormat,
                    BusinessSoftwareName = _businessSoftwareName,
                    CryptoSoftPath = _cryptoSoftPath,
                    EncryptionExtensions = _encryptionExtensions,
                    PriorityExtensions = _priorityExtensions,
                    LargeFileThreshold = _largeFileThreshold,
                    MaxParallelJobs = _maxParallelJobs,
                    LogCentralization = _logCentralizationSettings
                };
                
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                lock (SettingsFileLock)
                {
                    WriteAllTextAtomic(_settingsFilePath, json);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        protected virtual void LoadLogFormat()
        {
            try
            {
                string logFormatPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logformat.txt");
                lock (LogFormatFileLock)
                {
                    if (File.Exists(logFormatPath))
                    {
                        string format = File.ReadAllText(logFormatPath).Trim();
                        if (format == "XML" || format == "JSON")
                        {
                            _logFormat = format;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading log format: {ex.Message}");
            }
        }

        protected virtual void SaveLogFormat()
        {
            try
            {
                string logFormatPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logformat.txt");
                lock (LogFormatFileLock)
                {
                    WriteAllTextAtomic(logFormatPath, _logFormat);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving log format: {ex.Message}");
            }
        }

        private static void WriteAllTextAtomic(string path, string content)
        {
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempPath = Path.Combine(directory ?? AppDomain.CurrentDomain.BaseDirectory, $"{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
            File.WriteAllText(tempPath, content);
            File.Move(tempPath, path, true);
        }

        // --- Helpers ---

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }

        private bool TryParseFileSize(string input, out long bytes)
        {
            bytes = 0;
            if (string.IsNullOrWhiteSpace(input))
                return false;
                
            input = input.Trim().ToUpperInvariant();
            
            // Extract the numeric part and the unit
            string numericPart = new string(input.TakeWhile(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
            string unit = input.Substring(numericPart.Length).Trim();
            
            if (!double.TryParse(numericPart.Replace(',', '.'), out double value))
                return false;
                
            // Convert to bytes based on the unit
            switch (unit)
            {
                case "B":
                    bytes = (long)value;
                    return true;
                case "KB":
                    bytes = (long)(value * 1024);
                    return true;
                case "MB":
                    bytes = (long)(value * 1024 * 1024);
                    return true;
                case "GB":
                    bytes = (long)(value * 1024 * 1024 * 1024);
                    return true;
                case "TB":
                    bytes = (long)(value * 1024 * 1024 * 1024 * 1024);
                    return true;
                default:
                    // If no unit is specified, assume bytes
                    bytes = (long)value;
                    return true;
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
                // Update all translation properties
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
                OnPropertyChanged(nameof(CryptoPasswordLabel));
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
                OnPropertyChanged(nameof(CryptoPasswordNote));
                OnPropertyChanged(nameof(LogCentralizationNote));
                OnPropertyChanged(nameof(ChangesNote));
                OnPropertyChanged(nameof(CloseButtonText));
                OnPropertyChanged(nameof(BrowseButtonText));
                OnPropertyChanged(nameof(LanguageLabel));
                OnPropertyChanged(nameof(EnglishLanguageText));
                OnPropertyChanged(nameof(FrenchLanguageText));
                OnPropertyChanged(nameof(SelectedLanguageCode));
            }
        }

        private string GetTranslation(string key)
        {
            return _translationService?.GetTranslation(key) ?? key;
        }

        private class SettingsModel
        {
            public string? Language { get; set; }
            public string? LogFormat { get; set; }
            public string? BusinessSoftwareName { get; set; }
            public string? CryptoSoftPath { get; set; }
            public List<string>? EncryptionExtensions { get; set; }
            public List<string>? PriorityExtensions { get; set; }
            public long LargeFileThreshold { get; set; }
            public int MaxParallelJobs { get; set; }
            public LogCentralizationSettings LogCentralization { get; set; }
        }
    }
}