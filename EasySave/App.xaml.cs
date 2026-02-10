using System;
using System.IO;
using System.Windows;
using EasySave.Services;
using EasySave.ViewModels;
using EasySave.Views;
using Microsoft.Extensions.DependencyInjection;
using EasyLog;

namespace EasySave
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application  // Spécifiez explicitement System.Windows.Application
    {
        private readonly ServiceProvider _serviceProvider;

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
            
            // Enregistrer le chemin du fichier d'état comme un service
            string stateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "state.json");
            services.AddSingleton(stateFilePath); // Enregistrer le chemin comme un service de type string
            
            // Ensuite enregistrer StateManager qui utilisera ce string
            services.AddSingleton<StateManager>();
            
            // Configure and register logger
            string logFormat = LoadLogFormat();
            ILogger logger = LoggerFactory.CreateLogger(logFormat);
            services.AddSingleton(logger);
            
            services.AddSingleton<BackupService>();
            services.AddSingleton<SettingsViewModel>();
            
            // Register ViewModels
            services.AddSingleton<MainViewModel>();
            
            // Register Views
            services.AddTransient<MainWindow>();
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
            Console.WriteLine("      If no arguments are provided, the graphical interface will be launched.");
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
    }
}