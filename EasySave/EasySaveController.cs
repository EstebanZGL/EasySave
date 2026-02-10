using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EasySave.Models;
using EasySave.Services;
using EasyLog;

namespace EasySave
{
    // Controller for EasySave application
    public class EasySaveController
    {
        private readonly TranslationService _translationService;
        private readonly BackupJobManager _jobManager;
        private readonly BackupService _backupService;
        private readonly StateManager _stateManager;
        private readonly ILogger _logger;
        private readonly Dictionary<string, DateTime> _lastBackupTimes;

        // Constructor
        public EasySaveController()
        {
            // Initialize services
            _translationService = new TranslationService();
            _stateManager = new StateManager("state.json"); // Ajout du chemin du fichier d'état
            
            // Load log format preference
            string logFormat = LoadLogFormat();
            _logger = LoggerFactory.CreateLogger(logFormat);
            
            _jobManager = new BackupJobManager();
            _backupService = new BackupService(_logger, _stateManager);
            _lastBackupTimes = new Dictionary<string, DateTime>();
            
            // Load last backup times
            LoadLastBackupTimes();
        }

        // Run the application with command line arguments
        public async Task RunWithArgsAsync(string[] args)
        {
            if (args.Length == 0)
            {
                // No arguments, show interactive menu
                await ShowMainMenuAsync();
            }
            else
            {
                // Parse and execute jobs based on arguments
                await ExecuteCommandLineArgsAsync(args);
            }
        }

        // Execute backup jobs based on command line arguments
        private async Task ExecuteCommandLineArgsAsync(string[] args)
        {
            try
            {
                // Parse job indexes from arguments
                List<int> jobIndexes = ParseJobIndexes(args[0]);
                
                if (jobIndexes.Count == 0)
                {
                    Console.WriteLine("No valid job indexes provided.");
                    DisplayCommandLineHelp();
                    return;
                }
                
                // Get all jobs
                var jobs = _jobManager.GetJobs();
                
                // Execute specified jobs
                foreach (int index in jobIndexes)
                {
                    // Convert from 1-based to 0-based index
                    int zeroBasedIndex = index - 1;
                    
                    if (zeroBasedIndex >= 0 && zeroBasedIndex < jobs.Count)
                    {
                        var job = jobs[zeroBasedIndex];
                        Console.WriteLine($"Executing job #{index}: {job.JobName}");
                        
                        try
                        {
                            await _backupService.ExecuteBackupJobAsync(job);
                            
                            // Update last backup time
                            _lastBackupTimes[job.JobName] = DateTime.Now;
                            SaveLastBackupTimes();
                            
                            Console.WriteLine($"Job #{index} completed successfully.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error executing job #{index}: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Job #{index} not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing job indexes: {ex.Message}");
                DisplayCommandLineHelp();
            }
        }

        // Parse job indexes from a string like "1;3-5"
        private List<int> ParseJobIndexes(string input)
        {
            var result = new List<int>();
            
            // Split by semicolon
            string[] parts = input.Split(';');
            
            foreach (string part in parts)
            {
                // Check if it's a range (contains '-')
                if (part.Contains('-'))
                {
                    string[] range = part.Split('-');
                    if (range.Length == 2 && 
                        int.TryParse(range[0], out int start) && 
                        int.TryParse(range[1], out int end))
                    {
                        // Add all numbers in the range
                        for (int i = start; i <= end; i++)
                        {
                            result.Add(i);
                        }
                    }
                }
                // Otherwise treat as a single number
                else if (int.TryParse(part, out int index))
                {
                    result.Add(index);
                }
            }
            
            return result;
        }

        // Display help for command line usage
        private void DisplayCommandLineHelp()
        {
            Console.WriteLine("EasySave 2.0 - Command Line Usage");
            Console.WriteLine("================================");
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("  EasySave.exe [options] [job_selection]");
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("  -h, --help, /?    Display this help information");
            Console.WriteLine();
            Console.WriteLine("JOB SELECTION:");
            Console.WriteLine("  Specify which backup jobs to execute using the following formats:");
            Console.WriteLine("  - Single job:      EasySave.exe 1");
            Console.WriteLine("  - Multiple jobs:   EasySave.exe 1;3;5");
            Console.WriteLine("  - Range of jobs:   EasySave.exe 1-3");
            Console.WriteLine("  - Combined:        EasySave.exe 1;3-5");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine("  EasySave.exe 1     Execute backup job #1");
            Console.WriteLine("  EasySave.exe 1;3   Execute backup jobs #1 and #3");
            Console.WriteLine("  EasySave.exe 1-3   Execute backup jobs #1, #2, and #3");
            Console.WriteLine();
            Console.WriteLine("NOTE: Job numbers refer to the position in the job list (starting from 1).");
        }

        // Show the main interactive menu
        private async Task ShowMainMenuAsync()
        {
            // This method is only used in console mode, which is no longer needed in v2.0
            // But we keep it for backward compatibility with command-line mode
            Console.WriteLine("EasySave 2.0 - Console Mode");
            Console.WriteLine("Please use the graphical interface for full functionality.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            await Task.CompletedTask;
        }

        // Load last backup times from file
        private void LoadLastBackupTimes()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lastbackuptimes.json");
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var times = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json);
                    if (times != null)
                    {
                        foreach (var entry in times)
                        {
                            _lastBackupTimes[entry.Key] = entry.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading last backup times: {ex.Message}");
            }
        }

        // Save last backup times to file
        private void SaveLastBackupTimes()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lastbackuptimes.json");
                string json = System.Text.Json.JsonSerializer.Serialize(_lastBackupTimes);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving last backup times: {ex.Message}");
            }
        }

        // Load log format preference from configuration file
        private string LoadLogFormat()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logformat.txt");
                if (File.Exists(configPath))
                {
                    string format = File.ReadAllText(configPath).Trim();
                    if (format == "XML" || format == "JSON")
                    {
                        return format;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading log format: {ex.Message}");
            }
            
            // Default to JSON if no preference is found or an error occurs
            return "JSON";
        }

        // Save log format preference to configuration file
        private void SaveLogFormat(string format)
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logformat.txt");
                File.WriteAllText(configPath, format);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving log format: {ex.Message}");
            }
        }
    }
}