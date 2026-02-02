using System;
using System.Threading.Tasks;

namespace EasySave
{
    /// <summary>
    /// Main entry point for EasySave application
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry point of the application
        /// </summary>
        /// <param name="args">Command line arguments for specifying backup jobs to execute</param>
        /// <returns>Task representing the asynchronous operation</returns>
        static async Task Main(string[] args)
        {
            try
            {
                // Create and run the controller
                var controller = new EasySaveController();
                await controller.RunWithArgsAsync(args);
            }
            catch (Exception ex)
            {
                // Global exception handler
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}