using System;
using System.Threading.Tasks;
using EasySave.Models;
using EasySave.Services;
using EasySave.ViewModels;
using EasyLog;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace EasySaveCLI
{
    class Program
    {
        private static ServiceProvider _serviceProvider = null!;

        static async Task Main(string[] args)
        {
            Console.WriteLine("EasySave CLI");
            Console.WriteLine("----------------");
            
            // Configure dependency injection
            ConfigureServices();
            
            try
            {
                // Create controller and run with arguments
                var controller = new EasySaveCLIController(_serviceProvider);
                await controller.RunWithArgsAsync(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // Dispose services
                if (_serviceProvider != null)
                {
                    _serviceProvider.Dispose();
                }
            }
        }

        private static void ConfigureServices()
        {
            // Create service collection
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<TranslationService>();
            
            // Définir un chemin absolu pour le fichier de jobs
            // Utiliser le même répertoire que l'application principale
            string jobsFilePath = Path.Combine(
                Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) ?? string.Empty,
                "backupjobs.json");
            
            // Afficher le chemin pour le débogage
            Console.WriteLine($"Using jobs file path: {jobsFilePath}");
            
            // Enregistrer BackupJobRepository avec le chemin spécifique
            services.AddSingleton<BackupJobRepository>(provider => 
                new BackupJobRepository(jobsFilePath));
            
            // Register StateManager with the state file path
            services.AddSingleton<StateManager>(provider => {
                // Define the state file path - use same directory as jobs file
                string stateFilePath = Path.Combine(
                    Path.GetDirectoryName(jobsFilePath) ?? string.Empty,
                    "state.json");
                Console.WriteLine($"Using state file path: {stateFilePath}");
                return new StateManager(stateFilePath);
            });
            
            services.AddSingleton<SettingsViewModel>();
            
            // Configure CryptoService
            services.AddSingleton<CryptoService>();
            
            // Configure logger based on settings
            services.AddSingleton<IEncryptionLogger>(provider => {
                // Get settings to determine log format
                var settingsPath = Path.Combine(
                    Path.GetDirectoryName(jobsFilePath) ?? string.Empty,
                    "logformat.txt");
                string logFormat = "JSON"; // Default
                
                if (File.Exists(settingsPath))
                {
                    try
                    {
                        logFormat = File.ReadAllText(settingsPath).Trim();
                        Console.WriteLine($"Using log format: {logFormat}");
                    }
                    catch
                    {
                        Console.WriteLine("Could not read log format, using default: JSON");
                    }
                }
                
                // Create appropriate logger using static methods and cast to IEncryptionLogger
                ILogger logger = logFormat.ToUpper() == "XML" 
                    ? LoggerFactory.CreateXmlLogger() 
                    : LoggerFactory.CreateJsonLogger();
                
                // Ensure the logger implements IEncryptionLogger or cast it
                if (logger is IEncryptionLogger encryptionLogger)
                {
                    return encryptionLogger;
                }
                
                // If we reach here, we need to handle the case where the logger doesn't implement IEncryptionLogger
                // This is a fallback that should not happen in normal circumstances
                throw new InvalidOperationException("Logger does not implement IEncryptionLogger");
            });
            
            // Register ParallelBackupService instead of BackupService
            services.AddSingleton<ParallelBackupService>(provider => {
                var logger = provider.GetRequiredService<IEncryptionLogger>();
                var stateManager = provider.GetRequiredService<StateManager>();
                var cryptoService = provider.GetRequiredService<CryptoService>();
                var settingsViewModel = provider.GetRequiredService<SettingsViewModel>();
                var backupJobRepository = provider.GetRequiredService<BackupJobRepository>();
                
                return new ParallelBackupService(
                    logger,
                    stateManager,
                    cryptoService,
                    settingsViewModel,
                    backupJobRepository);
            });

            // Build service provider
            _serviceProvider = services.BuildServiceProvider();
        }
    }
}