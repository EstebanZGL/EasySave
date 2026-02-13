using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EasySave.Models;
using EasySave.Services;
using EasySave.ViewModels;
using EasyLog;
using Microsoft.Extensions.DependencyInjection;

namespace EasySaveCLI
{
    public class EasySaveCLIController
    {
        private readonly TranslationService _translationService;
        private readonly BackupJobManager _jobManager;
        private readonly BackupService _backupService;
        private readonly StateManager _stateManager;
        private readonly IEncryptionLogger _logger;
        private readonly SettingsViewModel _settings;
        private readonly Dictionary<string, DateTime> _lastBackupTimes;

        // Constructor
        public EasySaveCLIController(ServiceProvider serviceProvider)
        {
            // Get services from DI container
            _translationService = serviceProvider.GetRequiredService<TranslationService>();
            _jobManager = serviceProvider.GetRequiredService<BackupJobManager>();
            _backupService = serviceProvider.GetRequiredService<BackupService>();
            _stateManager = serviceProvider.GetRequiredService<StateManager>();
            _logger = serviceProvider.GetRequiredService<IEncryptionLogger>();
            _settings = serviceProvider.GetRequiredService<SettingsViewModel>();
            
            // Initialize last backup times
            _lastBackupTimes = new Dictionary<string, DateTime>();
            LoadLastBackupTimes();
        }

        // Run the application with command line arguments
        public async Task RunWithArgsAsync(string[] args)
        {
            if (args.Length == 0)
            {
                // No arguments, show interactive menu
                await ShowMainMenuAsync();
                return;
            }

            // Check for help command
            if (args[0] == "-h" || args[0] == "--help" || args[0] == "/?")
            {
                DisplayCommandLineHelp();
                return;
            }

            // Check for log format option
            if (args[0] == "--log" && args.Length > 1)
            {
                string format = args[1].ToUpper();
                if (format == "JSON" || format == "XML")
                {
                    // Utiliser la propriété LogFormat qui gère la sauvegarde en interne
                    _settings.LogFormat = format;
                    Console.WriteLine($"Log format set to {format}");
                    
                    // If there are more arguments, process them
                    if (args.Length > 2)
                    {
                        string[] remainingArgs = new string[args.Length - 2];
                        Array.Copy(args, 2, remainingArgs, 0, args.Length - 2);
                        await ExecuteCommandLineArgsAsync(remainingArgs);
                    }
                    return;
                }
                else
                {
                    Console.WriteLine("Invalid log format. Use JSON or XML.");
                    return;
                }
            }

            // Check for business software option
            if (args[0] == "--business-software" && args.Length > 1)
            {
                // Utiliser la propriété BusinessSoftwareName qui gère la sauvegarde en interne
                _settings.BusinessSoftwareName = args[1];
                Console.WriteLine($"Business software name set to {args[1]}");
                
                // If there are more arguments, process them
                if (args.Length > 2)
                {
                    string[] remainingArgs = new string[args.Length - 2];
                    Array.Copy(args, 2, remainingArgs, 0, args.Length - 2);
                    await ExecuteCommandLineArgsAsync(remainingArgs);
                }
                return;
            }

            // Check for encryption extensions option
            if (args[0] == "--encrypt-extensions" && args.Length > 1)
            {
                // Utiliser la propriété EncryptionExtensionsString qui gère la sauvegarde en interne
                _settings.EncryptionExtensionsString = args[1];
                Console.WriteLine($"Extensions to encrypt set to {args[1]}");
                
                // If there are more arguments, process them
                if (args.Length > 2)
                {
                    string[] remainingArgs = new string[args.Length - 2];
                    Array.Copy(args, 2, remainingArgs, 0, args.Length - 2);
                    await ExecuteCommandLineArgsAsync(remainingArgs);
                }
                return;
            }

            // Check for cryptosoft path option
            if (args[0] == "--cryptosoft-path" && args.Length > 1)
            {
                // Utiliser la propriété CryptoSoftPath qui gère la sauvegarde en interne
                _settings.CryptoSoftPath = args[1];
                Console.WriteLine($"CryptoSoft path set to {args[1]}");
                
                // If there are more arguments, process them
                if (args.Length > 2)
                {
                    string[] remainingArgs = new string[args.Length - 2];
                    Array.Copy(args, 2, remainingArgs, 0, args.Length - 2);
                    await ExecuteCommandLineArgsAsync(remainingArgs);
                }
                return;
            }

            // Process job selection arguments
            await ExecuteCommandLineArgsAsync(args);
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
                        var jobToExecute = jobs[zeroBasedIndex];
                        Console.WriteLine($"Executing job #{index}: {jobToExecute.JobName}");
                        
                        try
                        {
                            // Check if business software is running
                            if (_settings.IsBusinessSoftwareRunning())
                            {
                                Console.WriteLine($"Cannot start backup: Business software ({_settings.BusinessSoftwareName}) is running.");
                                continue;
                            }

                            await _backupService.ExecuteBackupJobAsync(jobToExecute);
                            
                            // Update last backup time
                            _lastBackupTimes[jobToExecute.JobName] = DateTime.Now;
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
            Console.WriteLine("EasySave 3.0 CLI - Command Line Usage");
            Console.WriteLine("====================================");
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("  EasySaveCLI.exe [options] [job_selection]");
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("  -h, --help, /?              Display this help information");
            Console.WriteLine("  --log <format>              Set log format (JSON or XML)");
            Console.WriteLine("  --business-software <name>  Set business software name to monitor");
            Console.WriteLine("  --encrypt-extensions <list> Set file extensions to encrypt (comma-separated)");
            Console.WriteLine("  --cryptosoft-path <path>    Set path to CryptoSoft executable");
            Console.WriteLine();
            Console.WriteLine("JOB SELECTION:");
            Console.WriteLine("  Specify which backup jobs to execute using the following formats:");
            Console.WriteLine("  - Single job:      EasySaveCLI.exe 1");
            Console.WriteLine("  - Multiple jobs:   EasySaveCLI.exe 1;3;5");
            Console.WriteLine("  - Range of jobs:   EasySaveCLI.exe 1-3");
            Console.WriteLine("  - Combined:        EasySaveCLI.exe 1;3-5");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine("  EasySaveCLI.exe 1                        Execute backup job #1");
            Console.WriteLine("  EasySaveCLI.exe --log XML 1;3            Set log format to XML and execute jobs #1 and #3");
            Console.WriteLine("  EasySaveCLI.exe --encrypt-extensions .txt,.pdf 1-3  Set extensions to encrypt and execute jobs #1, #2, and #3");
            Console.WriteLine();
            Console.WriteLine("NOTE: Job numbers refer to the position in the job list (starting from 1).");
        }

        // Show the main interactive menu
        private async Task ShowMainMenuAsync()
        {
            bool exit = false;
            
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("EasySave 3.0 CLI - Main Menu");
                Console.WriteLine("===========================");
                Console.WriteLine();
                Console.WriteLine("1. List backup jobs");
                Console.WriteLine("2. Create backup job");
                Console.WriteLine("3. Delete backup job");
                Console.WriteLine("4. Execute backup job");
                Console.WriteLine("5. Execute all backup jobs");
                Console.WriteLine("6. Change language (current: " + _translationService.CurrentLanguage + ")");
                Console.WriteLine("7. Change log format (current: " + _settings.LogFormat + ")");
                Console.WriteLine("8. Configure business software monitoring");
                Console.WriteLine("9. Configure encryption settings");
                Console.WriteLine("0. Exit");
                Console.WriteLine();
                Console.Write("Enter your choice: ");
                
                string choice = Console.ReadLine() ?? "";
                
                switch (choice)
                {
                    case "1":
                        ListBackupJobs();
                        break;
                    case "2":
                        await CreateBackupJobAsync();
                        break;
                    case "3":
                        await DeleteBackupJobAsync();
                        break;
                    case "4":
                        await ExecuteBackupJobAsync();
                        break;
                    case "5":
                        await ExecuteAllBackupJobsAsync();
                        break;
                    case "6":
                        ChangeLanguage();
                        break;
                    case "7":
                        ChangeLogFormat();
                        break;
                    case "8":
                        ConfigureBusinessSoftware();
                        break;
                    case "9":
                        ConfigureEncryption();
                        break;
                    case "0":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
            
            await Task.CompletedTask;
        }

        // List all backup jobs
        private void ListBackupJobs()
        {
            Console.Clear();
            Console.WriteLine("Backup Jobs");
            Console.WriteLine("===========");
            Console.WriteLine();
            
            var jobs = _jobManager.GetJobs();
            
            if (jobs.Count == 0)
            {
                Console.WriteLine("No backup jobs found.");
            }
            else
            {
                Console.WriteLine("ID | Name | Source | Target | Type | Last Backup");
                Console.WriteLine("---|------|--------|--------|------|------------");
                
                for (int i = 0; i < jobs.Count; i++)
                {
                    var currentJob = jobs[i];
                    string lastBackup = _lastBackupTimes.TryGetValue(currentJob.JobName, out DateTime time) 
                        ? time.ToString("yyyy-MM-dd HH:mm:ss") 
                        : "Never";
                    
                    Console.WriteLine($"{i + 1} | {currentJob.JobName} | {currentJob.SourcePath} | {currentJob.TargetPath} | {currentJob.Type} | {lastBackup}");
                }
            }
            
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        // Create a new backup job
        private async Task CreateBackupJobAsync()
        {
            Console.Clear();
            Console.WriteLine("Create Backup Job");
            Console.WriteLine("================");
            Console.WriteLine();
            
            // Get job name
            Console.Write("Enter job name: ");
            string name = Console.ReadLine() ?? "";
            
            // Get source path
            Console.Write("Enter source path: ");
            string source = Console.ReadLine() ?? "";
            
            // Get target path
            Console.Write("Enter target path: ");
            string target = Console.ReadLine() ?? "";
            
            // Get backup type
            Console.WriteLine("Select backup type:");
            Console.WriteLine("1. Complete");
            Console.WriteLine("2. Differential");
            Console.Write("Enter your choice (1-2): ");
            string typeChoice = Console.ReadLine() ?? "1";
            
            BackupType type = typeChoice == "2" ? BackupType.Differential : BackupType.Complete;
            
            // Create job
            var job = new BackupJob
            {
                JobName = name,
                SourcePath = source,
                TargetPath = target,
                Type = type
            };
            
            // Validate job
            if (!job.Validate())
            {
                Console.WriteLine("Invalid job configuration. Please check your inputs.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            
            // Save job
            _jobManager.CreateJob(job);
            await _logger.LogApplicationEventAsync("JobCreated", $"Created backup job: {job.JobName}");
            
            Console.WriteLine("Backup job created successfully.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        // Delete a backup job
        private async Task DeleteBackupJobAsync()
        {
            Console.Clear();
            Console.WriteLine("Delete Backup Job");
            Console.WriteLine("================");
            Console.WriteLine();
            
            var jobs = _jobManager.GetJobs();
            
            if (jobs.Count == 0)
            {
                Console.WriteLine("No backup jobs found.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            
            // List jobs
            Console.WriteLine("ID | Name | Source | Target | Type");
            Console.WriteLine("---|------|--------|--------|-----");
            
            for (int i = 0; i < jobs.Count; i++)
            {
                var currentJob = jobs[i];
                Console.WriteLine($"{i + 1} | {currentJob.JobName} | {currentJob.SourcePath} | {currentJob.TargetPath} | {currentJob.Type}");
            }
            
            Console.WriteLine();
            Console.Write("Enter the ID of the job to delete (0 to cancel): ");
            
            if (!int.TryParse(Console.ReadLine(), out int id) || id < 1 || id > jobs.Count)
            {
                Console.WriteLine("Invalid job ID.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            
            // Convert from 1-based to 0-based index
            int index = id - 1;
            
            // Delete job
            var jobToDelete = jobs[index];
            _jobManager.DeleteJob(jobToDelete.JobName);
            await _logger.LogApplicationEventAsync("JobDeleted", $"Deleted backup job: {jobToDelete.JobName}");
            
            // Remove from last backup times
            if (_lastBackupTimes.ContainsKey(jobToDelete.JobName))
            {
                _lastBackupTimes.Remove(jobToDelete.JobName);
                SaveLastBackupTimes();
            }
            
            Console.WriteLine("Backup job deleted successfully.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        // Execute a backup job
        private async Task ExecuteBackupJobAsync()
        {
            Console.Clear();
            Console.WriteLine("Execute Backup Job");
            Console.WriteLine("=================");
            Console.WriteLine();
            
            var jobs = _jobManager.GetJobs();
            
            if (jobs.Count == 0)
            {
                Console.WriteLine("No backup jobs found.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            
            // List jobs
            Console.WriteLine("ID | Name | Source | Target | Type");
            Console.WriteLine("---|------|--------|--------|-----");
            
            for (int i = 0; i < jobs.Count; i++)
            {
                var currentJob = jobs[i];
                Console.WriteLine($"{i + 1} | {currentJob.JobName} | {currentJob.SourcePath} | {currentJob.TargetPath} | {currentJob.Type}");
            }
            
            Console.WriteLine();
            Console.Write("Enter the ID of the job to execute (0 to cancel): ");
            
            if (!int.TryParse(Console.ReadLine(), out int id) || id < 1 || id > jobs.Count)
            {
                Console.WriteLine("Invalid job ID.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            
            // Convert from 1-based to 0-based index
            int index = id - 1;
            
            // Execute job
            var jobToExecute = jobs[index];
            
            // Check if business software is running
            if (_settings.IsBusinessSoftwareRunning())
            {
                Console.WriteLine($"Cannot start backup: Business software ({_settings.BusinessSoftwareName}) is running.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine($"Executing job: {jobToExecute.JobName}");
            
            try
            {
                await _backupService.ExecuteBackupJobAsync(jobToExecute);
                
                // Update last backup time
                _lastBackupTimes[jobToExecute.JobName] = DateTime.Now;
                SaveLastBackupTimes();
                
                Console.WriteLine("Backup job executed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing backup job: {ex.Message}");
            }
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        // Execute all backup jobs
        private async Task ExecuteAllBackupJobsAsync()
        {
            Console.Clear();
            Console.WriteLine("Execute All Backup Jobs");
            Console.WriteLine("=====================");
            Console.WriteLine();
            
            var jobs = _jobManager.GetJobs();
            
            if (jobs.Count == 0)
            {
                Console.WriteLine("No backup jobs found.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            
            // Check if business software is running
            if (_settings.IsBusinessSoftwareRunning())
            {
                Console.WriteLine($"Cannot start backup: Business software ({_settings.BusinessSoftwareName}) is running.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            
            // Execute all jobs
            int successCount = 0;
            int failCount = 0;
            
            foreach (var currentJob in jobs)
            {
                Console.WriteLine($"Executing job: {currentJob.JobName}");
                
                try
                {
                    await _backupService.ExecuteBackupJobAsync(currentJob);
                    
                    // Update last backup time
                    _lastBackupTimes[currentJob.JobName] = DateTime.Now;
                    
                    Console.WriteLine($"Job {currentJob.JobName} completed successfully.");
                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing job {currentJob.JobName}: {ex.Message}");
                    failCount++;
                }
                
                Console.WriteLine();
            }
            
            // Save last backup times
            SaveLastBackupTimes();
            
            Console.WriteLine($"Execution complete: {successCount} succeeded, {failCount} failed.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        // Change language
        private void ChangeLanguage()
        {
            Console.Clear();
            Console.WriteLine("Change Language");
            Console.WriteLine("==============");
            Console.WriteLine();
            Console.WriteLine("1. English");
            Console.WriteLine("2. French");
            Console.WriteLine();
            Console.Write("Enter your choice (1-2): ");
            
            string choice = Console.ReadLine() ?? "1";
            
            switch (choice)
            {
                case "1":
                    _translationService.SetLanguage("en");
                    Console.WriteLine("Language changed to English.");
                    break;
                case "2":
                    _translationService.SetLanguage("fr");
                    Console.WriteLine("Language changed to French.");
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        // Change log format
        private void ChangeLogFormat()
        {
            Console.Clear();
            Console.WriteLine("Change Log Format");
            Console.WriteLine("================");
            Console.WriteLine();
            Console.WriteLine("1. JSON");
            Console.WriteLine("2. XML");
            Console.WriteLine();
            Console.Write("Enter your choice (1-2): ");
            
            string choice = Console.ReadLine() ?? "1";
            
            switch (choice)
            {
                case "1":
                    _settings.LogFormat = "JSON";
                    Console.WriteLine("Log format changed to JSON.");
                    break;
                case "2":
                    _settings.LogFormat = "XML";
                    Console.WriteLine("Log format changed to XML.");
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        // Configure business software monitoring
        private void ConfigureBusinessSoftware()
        {
            Console.Clear();
            Console.WriteLine("Configure Business Software Monitoring");
            Console.WriteLine("====================================");
            Console.WriteLine();
            Console.WriteLine($"Current business software name: {_settings.BusinessSoftwareName}");
            Console.WriteLine();
            Console.Write("Enter new business software name (leave empty to keep current): ");
            
            string name = Console.ReadLine() ?? "";
            
            if (!string.IsNullOrEmpty(name))
            {
                _settings.BusinessSoftwareName = name;
                Console.WriteLine("Business software name updated.");
            }
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        // Configure encryption settings
        private void ConfigureEncryption()
        {
            Console.Clear();
            Console.WriteLine("Configure Encryption Settings");
            Console.WriteLine("===========================");
            Console.WriteLine();
            Console.WriteLine($"Current CryptoSoft path: {_settings.CryptoSoftPath}");
            Console.WriteLine($"Current extensions to encrypt: {_settings.EncryptionExtensionsString}");
            Console.WriteLine();
            
            // Update CryptoSoft path
            Console.Write("Enter new CryptoSoft path (leave empty to keep current): ");
            string path = Console.ReadLine() ?? "";
            
            if (!string.IsNullOrEmpty(path))
            {
                _settings.CryptoSoftPath = path;
            }
            
            // Update extensions to encrypt
            Console.Write("Enter new extensions to encrypt (comma-separated, leave empty to keep current): ");
            string extensions = Console.ReadLine() ?? "";
            
            if (!string.IsNullOrEmpty(extensions))
            {
                _settings.EncryptionExtensionsString = extensions;
            }
            
            Console.WriteLine("Encryption settings updated.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
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
    }
}