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
            services.AddSingleton<BackupJobManager>();
            
            // Register StateManager with the state file path
            services.AddSingleton<StateManager>(provider => {
                // Define the state file path
                string stateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "state.json");
                return new StateManager(stateFilePath);
            });
            
            services.AddSingleton<SettingsViewModel>();
            
            // Configure logger based on settings
            services.AddSingleton<IEncryptionLogger>(provider => {
                // Get settings to determine log format
                var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logformat.txt");
                string logFormat = "JSON"; // Default
                
                if (File.Exists(settingsPath))
                {
                    try
                    {
                        logFormat = File.ReadAllText(settingsPath).Trim();
                    }
                    catch
                    {
                        // Use default if file can't be read
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
            
            // Register backup service with dependencies
            services.AddSingleton<BackupService>(provider => {
                var stateManager = provider.GetRequiredService<StateManager>();
                var logger = provider.GetRequiredService<IEncryptionLogger>();
                
                // Use the constructor that accepts IEncryptionLogger and StateManager
                return new BackupService(logger, stateManager);
            });

            // Build service provider
            _serviceProvider = services.BuildServiceProvider();
        }
    }
}