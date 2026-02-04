using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using EasySave.Models;

namespace EasySave.Services
{
    // Manages backup jobs
    public class BackupJobManager
    {
        private const int MaxJobs = 5; // Maximum number of backup jobs allowed
        private readonly List<BackupJob> _backupJobs;
        private readonly string _configFilePath;
        private readonly object _lockObject = new object(); // For thread safety

        // Constructor
        public BackupJobManager(string configFilePath)
        {
            _configFilePath = configFilePath;
            _backupJobs = new List<BackupJob>();
            
            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Load existing jobs if available
            LoadJobs();
        }

        // Get all backup jobs
        public IReadOnlyList<BackupJob> GetJobs()
        {
            lock (_lockObject)
            {
                return _backupJobs.AsReadOnly();
            }
        }

        // Get a backup job by index
        public BackupJob GetJob(int index)
        {
            lock (_lockObject)
            {
                if (index >= 0 && index < _backupJobs.Count)
                {
                    return _backupJobs[index];
                }
                return null;
            }
        }

        // Create a new backup job
        // Returns: True if job was created, false otherwise
        public bool CreateJob(string name, string sourcePath, string targetPath, BackupType type)
        {
            lock (_lockObject)
            {
                // Check if maximum number of jobs has been reached
                if (_backupJobs.Count >= MaxJobs)
                {
                    return false;
                }
                
                // Create and validate the job
                var job = new BackupJob(name, sourcePath, targetPath, type);
                if (!job.Validate())
                {
                    return false;
                }
                
                // Add the job and save
                _backupJobs.Add(job);
                SaveJobs();
                
                return true;
            }
        }

        // Delete a backup job
        // Returns: True if job was deleted, false otherwise
        public bool DeleteJob(int index)
        {
            lock (_lockObject)
            {
                if (index >= 0 && index < _backupJobs.Count)
                {
                    _backupJobs.RemoveAt(index);
                    SaveJobs();
                    return true;
                }
                return false;
            }
        }

        // Load backup jobs from the configuration file
        private void LoadJobs()
        {
            if (File.Exists(_configFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_configFilePath);
                    var jobs = JsonSerializer.Deserialize<List<BackupJob>>(json);
                    if (jobs != null)
                    {
                        _backupJobs.Clear();
                        _backupJobs.AddRange(jobs);
                        
                        // Ensure we don't exceed the maximum number of jobs
                        if (_backupJobs.Count > MaxJobs)
                        {
                            _backupJobs.RemoveRange(MaxJobs, _backupJobs.Count - MaxJobs);
                            SaveJobs(); // Save the truncated list
                        }
                    }
                }
                catch
                {
                    // If loading fails, start with an empty list
                    _backupJobs.Clear();
                }
            }
        }

        // Save backup jobs to the configuration file
        private void SaveJobs()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_backupJobs, options);
                
                // Use atomic write operation to prevent file corruption
                string tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, json);
                File.Move(tempFile, _configFilePath, true);
            }
            catch
            {
                // Handle serialization or file access errors
            }
        }
    }
}