using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasySave.Models;
using EasySave.Services;
using EasyLog;

namespace EasySave
{
    /// <summary>
    /// Main controller for the EasySave application
    /// </summary>
    public class EasySaveController
    {
        private readonly BackupJobManager _jobManager;
        private readonly BackupService _backupService;
        private readonly TranslationService _translationService;
        private readonly ILogger _logger;
        private readonly StateManager _stateManager;
        private readonly string _appDataPath;
        private readonly Dictionary<string, DateTime> _lastBackupTimes;

        /// <summary>
        /// Constructor that initializes all required services
        /// </summary>
        public EasySaveController()
        {
            // Initialize application data path
            _appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EasySave");
            
            // Create required directories
            Directory.CreateDirectory(_appDataPath);
            string logDirectory = Path.Combine(_appDataPath, "logs");
            Directory.CreateDirectory(logDirectory);
            
            // Initialize services
            _translationService = new TranslationService();
            _logger = new JsonLogger(logDirectory);
            _stateManager = new StateManager(Path.Combine(_appDataPath, "state.json"));
            _jobManager = new BackupJobManager(Path.Combine(_appDataPath, "config.json"));
            _backupService = new BackupService(_logger, _stateManager);
            
            // Initialize last backup times tracking
            _lastBackupTimes = new Dictionary<string, DateTime>();
        }

        /// <summary>
        /// Entry point for application execution
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public async Task RunWithArgsAsync(string[] args)
        {
            if (args.Length > 0)
            {
                // Check if we need to show help
                if (args[0] == "-h" || args[0] == "--help" || args[0] == "/?")
                {
                    DisplayCommandLineHelp();
                    return;
                }
                
                // Execute specified jobs directly
                // We now support multiple arguments for PowerShell compatibility
                await ExecuteCommandLineArgsAsync(args);
            }
            else
            {
                // Show interactive menu
                await ShowMainMenuAsync();
            }
        }

        /// <summary>
        /// Displays command line help information
        /// </summary>
        private void DisplayCommandLineHelp()
        {
            Console.WriteLine("EasySave 1.0 - Command Line Usage");
            Console.WriteLine("================================");
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("  EasySave.exe [options] [job_numbers]");
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("  -h, --help, /?    Display this help information");
            Console.WriteLine();
            Console.WriteLine("JOB SELECTION:");
            Console.WriteLine("  Specify which backup jobs to execute using the following formats:");
            Console.WriteLine();
            Console.WriteLine("  In CMD:");
            Console.WriteLine("  - Single job:      EasySave.exe 1");
            Console.WriteLine("  - Multiple jobs:   EasySave.exe 1;3;5");
            Console.WriteLine("  - Range of jobs:   EasySave.exe 1-3");
            Console.WriteLine("  - Combined:        EasySave.exe 1;3-5");
            Console.WriteLine();
            Console.WriteLine("  In PowerShell:");
            Console.WriteLine("  - Single job:      .\\EasySave.exe 1");
            Console.WriteLine("  - Multiple jobs:   .\\EasySave.exe 1 3 5");
            Console.WriteLine("  - Range of jobs:   .\\EasySave.exe \"1-3\"");
            Console.WriteLine("  - Using semicolon: .\\EasySave.exe \"1;3;5\"");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine("  EasySave.exe 1     Execute backup job #1");
            Console.WriteLine("  EasySave.exe 1 3   Execute backup jobs #1 and #3");
            Console.WriteLine();
            Console.WriteLine("NOTE: Job numbers refer to the position in the job list (starting from 1).");
            Console.WriteLine("      If no arguments are provided, the interactive menu will be displayed.");
        }

        /// <summary>
        /// Displays the main application menu
        /// </summary>
        private async Task ShowMainMenuAsync()
        {
            bool exit = false;
            
            while (!exit)
            {
                Console.Clear();
                
                // Display menu
                Console.WriteLine(_translationService.GetTranslation("app_title"));
                Console.WriteLine("---------------------------");
                Console.WriteLine(_translationService.GetTranslation("menu_title"));
                Console.WriteLine("---------------------------");
                Console.WriteLine(_translationService.GetTranslation("menu_create"));
                Console.WriteLine(_translationService.GetTranslation("menu_execute"));
                Console.WriteLine(_translationService.GetTranslation("menu_list"));
                Console.WriteLine(_translationService.GetTranslation("menu_language"));
                Console.WriteLine(_translationService.GetTranslation("menu_exit"));
                Console.WriteLine("---------------------------");
                Console.Write(_translationService.GetTranslation("menu_choice"));
                
                string choice = Console.ReadLine();
                
                // Process selection
                switch (choice)
                {
                    case "1":
                        await CreateBackupJobAsync();
                        break;
                    case "2":
                        await ExecuteBackupJobsAsync();
                        break;
                    case "3":
                        await ManageBackupJobsAsync();
                        break;
                    case "4":
                        ChangeLanguage();
                        break;
                    case "5":
                        exit = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Creates a new backup job based on user input
        /// </summary>
        private async Task CreateBackupJobAsync()
        {
            Console.Clear();
            Console.WriteLine(_translationService.GetTranslation("create_title"));
            Console.WriteLine("---------------------------");
            
            // Check maximum job limit
            if (_jobManager.GetJobs().Count >= 5)
            {
                Console.WriteLine(_translationService.GetTranslation("create_max_reached"));
                Console.WriteLine(_translationService.GetTranslation("press_any_key"));
                Console.ReadKey();
                return;
            }
            
            // Collect job parameters
            Console.Write(_translationService.GetTranslation("create_name"));
            string name = Console.ReadLine();
            
            Console.Write(_translationService.GetTranslation("create_source"));
            string sourcePath = Console.ReadLine();
            
            Console.Write(_translationService.GetTranslation("create_target"));
            string targetPath = Console.ReadLine();
            
            Console.Write(_translationService.GetTranslation("create_type"));
            string typeInput = Console.ReadLine();
            
            BackupType type = typeInput == "2" ? BackupType.Differential : BackupType.Complete;
            
            // Create the job
            bool success = _jobManager.CreateJob(name, sourcePath, targetPath, type);
            
            if (success)
            {
                Console.WriteLine(_translationService.GetTranslation("create_success"));
            }
            else
            {
                Console.WriteLine(_translationService.GetTranslation("create_error"));
            }
            
            Console.WriteLine(_translationService.GetTranslation("press_any_key"));
            Console.ReadKey();
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Executes one or more backup jobs selected by the user
        /// </summary>
        private async Task ExecuteBackupJobsAsync()
        {
            var jobs = _jobManager.GetJobs();
            
            if (jobs.Count == 0)
            {
                Console.WriteLine(_translationService.GetTranslation("execute_no_jobs"));
                Console.WriteLine(_translationService.GetTranslation("press_any_key"));
                Console.ReadKey();
                return;
            }
            
            Console.Clear();
            Console.WriteLine(_translationService.GetTranslation("execute_title"));
            Console.WriteLine("---------------------------");
            
            // List available jobs
            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {jobs[i].Name} ({jobs[i].Type})");
            }
            
            Console.WriteLine("---------------------------");
            Console.Write(_translationService.GetTranslation("execute_select"));
            string input = Console.ReadLine();
            
            // Parse selection
            List<int> jobIndexes = ParseJobIndexes(input);
            
            // Execute selected jobs
            foreach (int index in jobIndexes)
            {
                if (index >= 0 && index < jobs.Count)
                {
                    try
                    {
                        await _backupService.ExecuteBackupJobAsync(jobs[index]);
                        
                        // Record the backup time
                        _lastBackupTimes[jobs[index].Name] = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
            
            Console.WriteLine(_translationService.GetTranslation("execute_success"));
            Console.WriteLine(_translationService.GetTranslation("press_any_key"));
            Console.ReadKey();
        }

        /// <summary>
        /// Manages backup jobs (list, view details, delete)
        /// </summary>
        private async Task ManageBackupJobsAsync()
        {
            var jobs = _jobManager.GetJobs();
            
            Console.Clear();
            Console.WriteLine(_translationService.GetTranslation("list_title"));
            Console.WriteLine("---------------------------");
            
            if (jobs.Count == 0)
            {
                Console.WriteLine(_translationService.GetTranslation("list_no_jobs"));
                Console.WriteLine(_translationService.GetTranslation("press_any_key"));
                Console.ReadKey();
                return;
            }
            
            // Display job details
            for (int i = 0; i < jobs.Count; i++)
            {
                var job = jobs[i];
                Console.WriteLine($"{i + 1}. {job.Name}");
                Console.WriteLine($"   {_translationService.GetTranslation("list_source")}{job.SourcePath}");
                Console.WriteLine($"   {_translationService.GetTranslation("list_target")}{job.TargetPath}");
                Console.WriteLine($"   {_translationService.GetTranslation("list_type")}{job.Type}");
                
                // Display last backup time if available
                if (_lastBackupTimes.TryGetValue(job.Name, out DateTime lastBackup))
                {
                    Console.WriteLine($"   {_translationService.GetTranslation("list_last_backup")}{lastBackup:yyyy-MM-dd HH:mm:ss}");
                }
                else
                {
                    Console.WriteLine($"   {_translationService.GetTranslation("list_last_backup")}{_translationService.GetTranslation("list_never")}");
                }
                
                Console.WriteLine();
            }
            
            // Show delete option
            Console.WriteLine("---------------------------");
            Console.WriteLine(_translationService.GetTranslation("list_delete_option"));
            Console.WriteLine(_translationService.GetTranslation("list_back_option"));
            Console.WriteLine("---------------------------");
            Console.Write(_translationService.GetTranslation("list_choice"));
            
            string input = Console.ReadLine();
            
            if (input.ToLower() == "d" || input.ToLower() == "delete")
            {
                await DeleteBackupJobAsync();
            }
            else
            {
                // Just return to main menu
                await Task.CompletedTask;
            }
        }

        /// <summary>
        /// Deletes a backup job selected by the user
        /// </summary>
        private async Task DeleteBackupJobAsync()
        {
            var jobs = _jobManager.GetJobs();
            
            Console.WriteLine("---------------------------");
            Console.Write(_translationService.GetTranslation("delete_select"));
            
            if (int.TryParse(Console.ReadLine(), out int jobNumber) && jobNumber >= 1 && jobNumber <= jobs.Count)
            {
                int index = jobNumber - 1;
                string jobName = jobs[index].Name;
                
                Console.Write(_translationService.GetTranslation("delete_confirm").Replace("{0}", jobName));
                string confirmation = Console.ReadLine();
                
                if (confirmation.ToLower() == "y" || confirmation.ToLower() == "yes" || 
                    confirmation.ToLower() == "o" || confirmation.ToLower() == "oui")
                {
                    bool success = _jobManager.DeleteJob(index);
                    
                    if (success)
                    {
                        // Remove from last backup times if exists
                        if (_lastBackupTimes.ContainsKey(jobName))
                        {
                            _lastBackupTimes.Remove(jobName);
                        }
                        
                        Console.WriteLine(_translationService.GetTranslation("delete_success"));
                    }
                    else
                    {
                        Console.WriteLine(_translationService.GetTranslation("delete_error"));
                    }
                }
                else
                {
                    Console.WriteLine(_translationService.GetTranslation("delete_cancelled"));
                }
            }
            else
            {
                Console.WriteLine(_translationService.GetTranslation("delete_invalid"));
            }
            
            Console.WriteLine(_translationService.GetTranslation("press_any_key"));
            Console.ReadKey();
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Changes the application's current language
        /// </summary>
        private void ChangeLanguage()
        {
            _translationService.ToggleLanguage();
            
            Console.WriteLine(_translationService.GetTranslation("language_changed"));
            Console.WriteLine(_translationService.GetTranslation("press_any_key"));
            Console.ReadKey();
        }

        /// <summary>
        /// Parses user input to determine which backup jobs to execute
        /// </summary>
        /// <param name="input">Input string (e.g., '1', '1-3', or '1;3')</param>
        /// <returns>List of job indexes</returns>
        private List<int> ParseJobIndexes(string input)
        {
            var indexes = new List<int>();
            
            if (string.IsNullOrWhiteSpace(input))
            {
                return indexes;
            }
            
            string[] parts = input.Split(';');
            
            foreach (string part in parts)
            {
                // Handle range format (e.g., "1-3")
                if (part.Contains('-'))
                {
                    string[] range = part.Split('-');
                    if (range.Length == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                    {
                        for (int i = start; i <= end; i++)
                        {
                            indexes.Add(i - 1); // Convert to 0-based index
                        }
                    }
                }
                // Handle single number
                else if (int.TryParse(part, out int index))
                {
                    indexes.Add(index - 1); // Convert to 0-based index
                }
            }
            
            return indexes;
        }

        /// <summary>
        /// Executes backup jobs specified by command line arguments
        /// </summary>
        /// <param name="args">Command line arguments</param>
        private async Task ExecuteCommandLineArgsAsync(string[] args)
        {
            var jobs = _jobManager.GetJobs();
            
            // Display available jobs in command line mode
            Console.WriteLine("EasySave 1.0 - Command Line Mode");
            Console.WriteLine("---------------------------");
            
            if (jobs.Count == 0)
            {
                Console.WriteLine("No backup jobs available. Please run the application without arguments to create jobs first.");
                return;
            }
            
            Console.WriteLine("Available backup jobs:");
            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {jobs[i].Name} ({jobs[i].Type})");
            }
            Console.WriteLine("---------------------------");
            
            // Process all arguments to support both PowerShell and CMD syntax
            var jobIndexes = new List<int>();
            
            foreach (string arg in args)
            {
                // Check if the argument is a simple number
                if (int.TryParse(arg, out int jobNumber))
                {
                    jobIndexes.Add(jobNumber - 1); // Convert to 0-based index
                }
                else
                {
                    // It might be a range or semicolon-separated list
                    jobIndexes.AddRange(ParseJobIndexes(arg));
                }
            }
            
            // Remove duplicates and sort
            jobIndexes = jobIndexes.Distinct().OrderBy(i => i).ToList();
            
            if (jobIndexes.Count == 0)
            {
                Console.WriteLine("No valid job numbers specified");
                Console.WriteLine("Usage examples:");
                Console.WriteLine("  In CMD:");
                Console.WriteLine("    EasySave.exe 1     (Execute job #1)");
                Console.WriteLine("    EasySave.exe 1;3   (Execute jobs #1 and #3)");
                Console.WriteLine();
                Console.WriteLine("  In PowerShell:");
                Console.WriteLine("    .\\EasySave.exe 1    (Execute job #1)");
                Console.WriteLine("    .\\EasySave.exe 1 3  (Execute jobs #1 and #3)");
                Console.WriteLine("    .\\EasySave.exe \"1;3\" (Execute jobs #1 and #3)");
                Console.WriteLine();
                Console.WriteLine("For more information, run: EasySave.exe --help");
                return;
            }
            
            // Show which jobs will be executed
            Console.WriteLine("Executing the following jobs:");
            foreach (int index in jobIndexes)
            {
                if (index >= 0 && index < jobs.Count)
                {
                    Console.WriteLine($"- {jobs[index].Name}");
                }
            }
            Console.WriteLine("---------------------------");
            
            // Execute selected jobs
            int successCount = 0;
            int failCount = 0;
            
            foreach (int index in jobIndexes)
            {
                if (index >= 0 && index < jobs.Count)
                {
                    try
                    {
                        Console.WriteLine($"Executing job: {jobs[index].Name}...");
                        await _backupService.ExecuteBackupJobAsync(jobs[index]);
                        
                        // Record the backup time
                        _lastBackupTimes[jobs[index].Name] = DateTime.Now;
                        
                        Console.WriteLine($"Job '{jobs[index].Name}' completed successfully.");
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing job '{jobs[index].Name}': {ex.Message}");
                        failCount++;
                    }
                }
                else
                {
                    Console.WriteLine($"Job #{index + 1} does not exist. Skipping.");
                }
            }
            
            // Display summary
            Console.WriteLine("---------------------------");
            Console.WriteLine($"Execution complete: {successCount} job(s) succeeded, {failCount} job(s) failed.");
        }
    }
}