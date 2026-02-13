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
        private string _businessSoftwareName;
        private List<string> _encryptionExtensions;
        private string _cryptoSoftPath;
        private int _maxParallelJobs;
        private string _logFormat;

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsViewModel()
        {
            // Default values
            _businessSoftwareName = "calc.exe";  // Default to Calculator for demo
            _encryptionExtensions = new List<string> { ".txt", ".doc", ".pdf" };
            _cryptoSoftPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CryptoSoft.exe");
            _maxParallelJobs = 5;
            _logFormat = "JSON"; // Default log format
            
            LoadSettings();
            LoadLogFormat(); // Load log format from separate file
            
            // Debug: Print the current business software name
            Debug.WriteLine($"Business software name set to: {_businessSoftwareName}");
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
                    _maxParallelJobs = value;
                    OnPropertyChanged();
                    SaveSettings();
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
                        EncryptionExtensions = settings.EncryptionExtensions;
                        CryptoSoftPath = settings.CryptoSoftPath;
                        MaxParallelJobs = settings.MaxParallelJobs;
                        
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
                    CryptoSoftPath = CryptoSoftPath,
                    MaxParallelJobs = MaxParallelJobs
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

        private class SettingsData
        {
            public string BusinessSoftwareName { get; set; }
            public List<string> EncryptionExtensions { get; set; }
            public string CryptoSoftPath { get; set; }
            public int MaxParallelJobs { get; set; }
        }
    }
}