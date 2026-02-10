using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsViewModel()
        {
            // Default values
            _businessSoftwareName = "calc.exe";  // Default to Calculator for demo
            _encryptionExtensions = new List<string> { ".txt", ".doc", ".pdf" };
            _cryptoSoftPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CryptoSoft.exe");
            _maxParallelJobs = 5;
            
            LoadSettings();
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

        public string EncryptionExtensionsString
        {
            get => string.Join(";", _encryptionExtensions);
            set
            {
                var extensions = value.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                EncryptionExtensions = new List<string>(extensions);
                OnPropertyChanged();
            }
        }

        public bool IsBusinessSoftwareRunning()
        {
            if (string.IsNullOrWhiteSpace(BusinessSoftwareName))
                return false;

            var processes = System.Diagnostics.Process.GetProcessesByName(
                Path.GetFileNameWithoutExtension(BusinessSoftwareName));
            
            return processes.Length > 0;
        }

        public bool ShouldEncryptFile(string filePath)
        {
            if (EncryptionExtensions == null || EncryptionExtensions.Count == 0)
                return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return EncryptionExtensions.Contains(extension);
        }

        private void LoadSettings()
        {
            try
            {
                string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
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
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with default settings
                Console.WriteLine($"Error loading settings: {ex.Message}");
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
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error saving settings: {ex.Message}");
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