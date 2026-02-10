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
        private readonly List<BackupJob> _backupJobs;
        private readonly string _configFilePath;
        private readonly object _lockObject = new object(); // For thread safety

        // Constructor
        public BackupJobManager()
        {
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jobs.json");
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

        // Get a backup job by name
        public BackupJob GetJob(string name)
        {
            lock (_lockObject)
            {
                return _backupJobs.Find(job => job.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
        }

        // Create a new backup job
        // Returns: Task completing when job is created
        public async Task CreateJob(BackupJob job)
        {
            if (job == null || !job.Validate())
            {
                throw new ArgumentException("Invalid backup job", nameof(job));
            }
            
            lock (_lockObject)
            {
                // Check if a job with the same name already exists
                if (_backupJobs.Exists(j => j.Name.Equals(job.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"A backup job with the name '{job.Name}' already exists.");
                }
                
                // Add the job and save
                _backupJobs.Add(job);
                SaveJobs();
            }

            // No need to return anything, but keeping the Task return type for async compatibility
            await Task.CompletedTask;
        }

        // Delete a backup job by name
        // Returns: Task completing when job is deleted
        public async Task DeleteJob(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Job name cannot be null or empty", nameof(name));
            }
            
            lock (_lockObject)
            {
                int index = _backupJobs.FindIndex(job => job.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (index < 0)
                {
                    throw new InvalidOperationException($"Backup job '{name}' not found.");
                }
                
                _backupJobs.RemoveAt(index);
                SaveJobs();
            }

            // No need to return anything, but keeping the Task return type for async compatibility
            await Task.CompletedTask;
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
                    }
                }
                catch (Exception ex)
                {
                    // If loading fails, start with an empty list
                    _backupJobs.Clear();
                    System.Diagnostics.Debug.WriteLine($"Error loading backup jobs: {ex.Message}");
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
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error saving backup jobs: {ex.Message}");
                throw; // Re-throw to let caller handle it
            }
        }
    }
}