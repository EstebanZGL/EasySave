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

        /// <summary>
        /// Constructor
        /// </summary>
        public EasySaveController()
        {
            // Initialize application data path
            _appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EasySave");
            
            // Create directories if they don't exist
            Directory.CreateDirectory(_appDataPath);
            string logDirectory = Path.Combine(_appDataPath, "logs");
            Directory.CreateDirectory(logDirectory);
            
            // Initialize services
            _translationService = new TranslationService();
            _logger = new JsonLogger(logDirectory);
            _stateManager = new StateManager(Path.Combine(_appDataPath, "state.json"));
            _jobManager = new BackupJobManager(Path.Combine(_appDataPath, "config.json"));
            _backupService = new BackupService(_logger, _stateManager);
        }

        /// <summary>
        /// Run the application with command line arguments
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task RunWithArgsAsync(string[] args)
        {
            if (args.Length > 0)
            {
                await ExecuteCommandLineArgsAsync(args[0]);
            }
            else
            {
                await ShowMainMenuAsync();
            }
        }

        /// <summary>
        /// Show the main menu
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task ShowMainMenuAsync()
        {
            bool exit = false;
            
            while (!exit)
            {
                Console.Clear();
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
                
                switch (choice)
                {
                    case "1":
                        await CreateBackupJobAsync();
                        break;
                    case "2":
                        await ExecuteBackupJobsAsync();
                        break;
                    case "3":
                        ListBackupJobs();
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
        /// Create a new backup job
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task CreateBackupJobAsync()
        {
            Console.Clear();
            Console.WriteLine(_translationService.GetTranslation("create_title"));
            Console.WriteLine("---------------------------");
            
            // Check if maximum number of jobs has been reached
            if (_jobManager.GetJobs().Count >= 5)
            {
                Console.WriteLine(_translationService.GetTranslation("create_max_reached"));
                Console.WriteLine(_translationService.GetTranslation("press_any_key"));
                Console.ReadKey();
                return;
            }
            
            Console.Write(_translationService.GetTranslation("create_name"));
            string name = Console.ReadLine();
            
            Console.Write(_translationService.GetTranslation("create_source"));
            string sourcePath = Console.ReadLine();
            
            Console.Write(_translationService.GetTranslation("create_target"));
            string targetPath = Console.ReadLine();
            
            Console.Write(_translationService.GetTranslation("create_type"));
            string typeInput = Console.ReadLine();
            BackupType type = typeInput == "2" ? BackupType.Differential : BackupType.Complete;
            
            // Create the backup job
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
            
            await Task.CompletedTask; // For async consistency
        }

        /// <summary>
        /// Execute backup job(s)
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
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
            
            // List available backup jobs
            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {jobs[i].Name} ({jobs[i].Type})");
            }
            
            Console.WriteLine("---------------------------");
            Console.Write(_translationService.GetTranslation("execute_select"));
            string input = Console.ReadLine();
            
            // Parse the input
            List<int> jobIndexes = ParseJobIndexes(input);
            
            // Execute the selected backup jobs
            foreach (int index in jobIndexes)
            {
                if (index >= 0 && index < jobs.Count)
                {
                    try
                    {
                        await _backupService.ExecuteBackupJobAsync(jobs[index]);
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
        /// List all backup jobs
        /// </summary>
        private void ListBackupJobs()
        {
            var jobs = _jobManager.GetJobs();
            
            Console.Clear();
            Console.WriteLine(_translationService.GetTranslation("list_title"));
            Console.WriteLine("---------------------------");
            
            if (jobs.Count == 0)
            {
                Console.WriteLine(_translationService.GetTranslation("list_no_jobs"));
            }
            else
            {
                for (int i = 0; i < jobs.Count; i++)
                {
                    var job = jobs[i];
                    Console.WriteLine($"{i + 1}. {job.Name}");
                    Console.WriteLine($"   {_translationService.GetTranslation("list_source")}{job.SourcePath}");
                    Console.WriteLine($"   {_translationService.GetTranslation("list_target")}{job.TargetPath}");
                    Console.WriteLine($"   {_translationService.GetTranslation("list_type")}{job.Type}");
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine(_translationService.GetTranslation("press_any_key"));
            Console.ReadKey();
        }

        /// <summary>
        /// Change the current language
        /// </summary>
        private void ChangeLanguage()
        {
            _translationService.ToggleLanguage();
            Console.WriteLine(_translationService.GetTranslation("language_changed"));
            Console.WriteLine(_translationService.GetTranslation("press_any_key"));
            Console.ReadKey();
        }

        /// <summary>
        /// Parse job indexes from input string
        /// </summary>
        /// <param name="input">Input string (e.g., '1', '1-3', or '1;3')</param>
        /// <returns>List of job indexes</returns>
        private List<int> ParseJobIndexes(string input)
        {
            var indexes = new List<int>();
            
            // Split by semicolon
            string[] parts = input.Split(';');
            
            foreach (string part in parts)
            {
                // Check if it's a range
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
                else if (int.TryParse(part, out int index))
                {
                    indexes.Add(index - 1); // Convert to 0-based index
                }
            }
            
            return indexes;
        }

        /// <summary>
        /// Execute backup jobs based on command line arguments
        /// </summary>
        /// <param name="arg">Command line argument</param>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task ExecuteCommandLineArgsAsync(string arg)
        {
            var jobs = _jobManager.GetJobs();
            
            // Parse the command line arguments
            List<int> jobIndexes = ParseJobIndexes(arg);
            
            // Execute the selected backup jobs
            foreach (int index in jobIndexes)
            {
                if (index >= 0 && index < jobs.Count)
                {
                    try
                    {
                        await _backupService.ExecuteBackupJobAsync(jobs[index]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
        }
    }
}