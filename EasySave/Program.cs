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
                // Display help if requested
                if (args.Length > 0 && (args[0] == "-h" || args[0] == "--help" || args[0] == "/?"))
                {
                    DisplayHelp();
                    return;
                }
                
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
        
        /// <summary>
        /// Displays command line help information
        /// </summary>
        private static void DisplayHelp()
        {
            Console.WriteLine("EasySave 1.0 - Command Line Usage");
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
            Console.WriteLine("      If no arguments are provided, the interactive menu will be displayed.");
        }
    }
}