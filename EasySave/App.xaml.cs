using System;
using System.IO;
using System.Windows;
using EasySave.Services;
using EasySave.ViewModels;
using EasySave.Views;
using Microsoft.Extensions.DependencyInjection;
using EasyLog;
using EasySave.Models;

namespace EasySave
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private readonly ServiceProvider _serviceProvider;

        // Expose le ServiceProvider pour permettre l'accès aux services depuis le XAML
        public ServiceProvider ServiceProvider => _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Register services
            services.AddSingleton<TranslationService>();
            services.AddSingleton<BackupJobManager>();
            
            // Enregistrer BackupJobRepository
            string backupJobsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backupjobs.json");
            services.AddSingleton<BackupJobRepository>(provider => new BackupJobRepository(backupJobsFilePath));
            
            // Enregistrer le chemin du fichier d'état comme un service
            string stateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "state.json");
            services.AddSingleton(stateFilePath); // Enregistrer le chemin comme un service de type string
            
            // Ensuite enregistrer StateManager qui utilisera ce string
            services.AddSingleton<StateManager>();
            
            // Register settings
            services.AddSingleton<SettingsViewModel>();
            
            // Configure and register logger
            string logFormat = LoadLogFormat();
            
            // Créer les répertoires de logs s'ils n'existent pas
            CreateLogDirectories();
            
            // Get log centralization settings
            var logCentralizationSettings = LoadLogCentralizationSettings();
            
            // Create the appropriate logger based on centralization settings
            IEncryptionLogger logger;
            if (logCentralizationSettings.IsEnabled)
            {
                // Create a logger with centralization support
                logger = LoggerFactory.CreateEncryptionLogger(
                    logFormat.ToLower(), 
                    null, // No specific file path, use default
                    logCentralizationSettings.ServerUrl,
                    logCentralizationSettings.LogDestination,
                    logCentralizationSettings.UserName); // Utiliser le nom d'utilisateur personnalisé
            }
            else
            {
                // Create a standard local logger without centralization
                logger = LoggerFactory.CreateEncryptionLogger(logFormat.ToLower(), null);
            }
            
            // Register the logger
            services.AddSingleton(logger);
            
            // Register crypto service
            services.AddSingleton<CryptoService>();
            
            // Register backup services
            services.AddSingleton<BackupService>();
            services.AddSingleton<ParallelBackupService>();
            
            // Register business software monitor with explicit factory to resolve ambiguity
            services.AddSingleton<BusinessSoftwareMonitor>(provider => {
                var settingsViewModel = provider.GetRequiredService<SettingsViewModel>();
                var parallelBackupService = provider.GetRequiredService<ParallelBackupService>();
                
                // Use the constructor with ParallelBackupService for version 3.0
                return new BusinessSoftwareMonitor(settingsViewModel, parallelBackupService);
            });
            
            // Register ViewModels
            services.AddSingleton<MainViewModel>();
            
            // Register Views
            // Modifier l'enregistrement de MainWindow pour utiliser une factory qui injecte les services nécessaires
            services.AddTransient<MainWindow>(provider => {
                var viewModel = provider.GetRequiredService<MainViewModel>();
                var translationService = provider.GetRequiredService<TranslationService>();
                var parallelBackupService = provider.GetRequiredService<ParallelBackupService>();
                
                return new MainWindow(viewModel, translationService, parallelBackupService);
            });
            services.AddTransient<BackupJobDialog>();
            services.AddTransient<SettingsWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Handle command-line arguments
            if (e.Args.Length > 0)
            {
                RunCommandLineMode(e.Args);
                Shutdown();
                return;
            }

            // Start the WPF application
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private async void RunCommandLineMode(string[] args)
        {
            try
            {
                // Display help if requested
                if (args.Length > 0 && (args[0] == "-h" || args[0] == "--help" || args[0] == "/?"))
                {
                    DisplayHelp();
                    return;
                }
                
                // Create and run the controller for command-line mode
                var controller = new EasySaveController();
                await controller.RunWithArgsAsync(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        
        // Displays command line help information
        private static void DisplayHelp()
        {
            Console.WriteLine("EasySave 3.0 - Command Line Usage");
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
            Console.WriteLine("      If no arguments are provided, the graphical interface will be launched.");
        }

        // Créer la structure de répertoires pour les logs
        private void CreateLogDirectories()
        {
            try
            {
                // Créer le répertoire principal des logs
                string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }

                // Créer le sous-répertoire pour les logs JSON
                string jsonDir = Path.Combine(logsDir, "Json");
                if (!Directory.Exists(jsonDir))
                {
                    Directory.CreateDirectory(jsonDir);
                }

                // Créer le sous-répertoire pour les logs XML
                string xmlDir = Path.Combine(logsDir, "Xml");
                if (!Directory.Exists(xmlDir))
                {
                    Directory.CreateDirectory(xmlDir);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating log directories: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"Error loading log format: {ex.Message}");
            }
            
            // Default to JSON if no preference is found or an error occurs
            return "JSON";
        }
        
        // Load log centralization settings
        private LogCentralizationSettings LoadLogCentralizationSettings()
        {
            try
            {
                string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    var settings = System.Text.Json.JsonSerializer.Deserialize<SettingsData>(json);
                    
                    if (settings?.LogCentralization != null)
                    {
                        return settings.LogCentralization;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading log centralization settings: {ex.Message}");
            }
            
            // Return default settings if no settings are found or an error occurs
            return new LogCentralizationSettings();
        }
        
        // Settings data class for deserialization
        private class SettingsData
        {
            public LogCentralizationSettings LogCentralization { get; set; }
        }
    }
}