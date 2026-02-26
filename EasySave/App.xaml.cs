using System;
using System.IO;
using System.Windows;
using EasySave.Services;
using EasySave.ViewModels;
using EasySave.Views;
using Microsoft.Extensions.DependencyInjection;
using EasyLog;
using EasySave.Models;
using System.Diagnostics;

namespace EasySave
{
     
    /// Interaction logic for App.xaml
     
    public partial class App : System.Windows.Application
    {
        private readonly ServiceProvider _serviceProvider;

        // Exposes the ServiceProvider to allow access to services from XAML
        public ServiceProvider ServiceProvider => _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            
            // Enregistrer d'abord SettingsViewModel pour pouvoir l'utiliser immédiatement
            services.AddSingleton<SettingsViewModel>();
            var tempProvider = services.BuildServiceProvider();
            var settingsViewModel = tempProvider.GetRequiredService<SettingsViewModel>();
            
            // Configurer LogIdentityProvider AVANT de créer les loggers
            var identityProvider = new SettingsLogIdentityProvider(settingsViewModel);
            LogIdentityProvider.Configure(identityProvider);
            
            // Maintenant configurer tous les autres services
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            
            // Reconfigurer LogIdentityProvider avec l'instance finale de SettingsViewModel
            settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
            identityProvider = new SettingsLogIdentityProvider(settingsViewModel);
            LogIdentityProvider.Configure(identityProvider);
            
            // Afficher les valeurs pour le débogage
            Debug.WriteLine($"App: CustomMachineName = '{settingsViewModel.CustomMachineName}'");
            Debug.WriteLine($"App: CustomUserName = '{settingsViewModel.CustomUserName}'");
            Debug.WriteLine($"App: LogIdentityProvider.GetMachineName() = '{LogIdentityProvider.GetMachineName()}'");
            Debug.WriteLine($"App: LogIdentityProvider.GetUserName() = '{LogIdentityProvider.GetUserName()}'");
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Register services
            services.AddSingleton<TranslationService>();
            services.AddSingleton<BackupJobManager>();
            
            // Register BackupJobRepository
            string backupJobsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backupjobs.json");
            services.AddSingleton<BackupJobRepository>(provider => new BackupJobRepository(backupJobsFilePath));
            
            // Register the state file path as a service
            string stateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "state.json");
            services.AddSingleton(stateFilePath); // Register the path as a string service
            
            // Then register StateManager which will use this string
            services.AddSingleton<StateManager>();
            
            // Configure and register logger
            string logFormat = LoadLogFormat();
            
            // Create log directories if they don't exist
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
                    logCentralizationSettings.LogDestination);
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
            // Modify MainWindow registration to use a factory that injects necessary services
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
                
                // LogIdentityProvider est déjà configuré dans le constructeur
                
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

        // Create the directory structure for logs
        private void CreateLogDirectories()
        {
            try
            {
                // Create the main logs directory
                string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }

                // Create the subdirectory for JSON logs
                string jsonDir = Path.Combine(logsDir, "Json");
                if (!Directory.Exists(jsonDir))
                {
                    Directory.CreateDirectory(jsonDir);
                }

                // Create the subdirectory for XML logs
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