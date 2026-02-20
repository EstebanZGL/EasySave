using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace CryptoSoft
{
    /// <summary>
    /// CryptoSoft - A simple file encryption utility designed to be called from EasySave.
    /// Version 4.0: Now with a simple GUI and mono-instance support.
    /// </summary>
    public class Program
    {
        private static readonly byte[] EncryptionKey =
        {
            0x42, 0x1A, 0xF3, 0x7B, 0x91, 0x30, 0x64, 0xE2,
            0x8B, 0x15, 0xD7, 0xAC, 0x56, 0x83, 0xC9, 0x48
        };

        private const int Success = 0;
        private const int ErrorInvalidArgs = 1;
        private const int ErrorFileNotFound = 2;
        private const int ErrorAccessDenied = 3;
        private const int ErrorEncryption = 4;
        private const int ErrorMutexTimeout = 5;

        // Name of the mutex to ensure single instance
        private const string MutexName = "Global\\CryptoSoft_SingleInstance_Mutex";
        
        // Timeout for acquiring the mutex (in milliseconds)
        private const int MutexTimeout = 30000; // 30 seconds

        // Global mutex reference
        private static Mutex? _mutex;
        private static bool _mutexCreated;

        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                // Try to create or open the named mutex
                _mutex = new Mutex(false, MutexName, out _mutexCreated);
                
                // Check if another instance is already running
                if (!_mutex.WaitOne(0, false))
                {
                    if (args.Length == 0)
                    {
                        MessageBox.Show("CryptoSoft is already running.", "CryptoSoft", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: Another instance of CryptoSoft is already running.");
                    }
                    return ErrorMutexTimeout;
                }

                try
                {
                    // If command-line arguments are provided, run in CLI mode
                    if (args.Length == 2)
                    {
                        return RunCliMode(args);
                    }
                    
                    // Otherwise, run in GUI mode
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new CryptoSoftForm());
                    
                    return Success;
                }
                finally
                {
                    // Always release the mutex when done
                    _mutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                if (args.Length == 0)
                {
                    MessageBox.Show($"Unexpected error: {ex.Message}", "CryptoSoft Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Console.Error.WriteLine($"Error: {ex.Message}");
                }
                return ErrorEncryption;
            }
            finally
            {
                // Dispose of the mutex
                _mutex?.Dispose();
            }
        }

        /// <summary>
        /// Runs CryptoSoft in command-line mode for compatibility with EasySave
        /// </summary>
        private static int RunCliMode(string[] args)
        {
            Console.WriteLine("CryptoSoft v4.0 - CLI Mode");
            
            string sourceFile = args[0];
            string targetFile = args[1];

            if (!File.Exists(sourceFile))
            {
                Console.Error.WriteLine($"Error: Source file not found: {sourceFile}");
                return ErrorFileNotFound;
            }

            Console.WriteLine($"Encrypting file: {Path.GetFileName(sourceFile)}");

            try
            {
                // Perform the encryption
                Stopwatch stopwatch = Stopwatch.StartNew();
                EncryptFile(sourceFile, targetFile);
                stopwatch.Stop();

                Console.WriteLine(stopwatch.ElapsedMilliseconds);
                return Success;
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine("Error: File not found");
                return ErrorFileNotFound;
            }
            catch (UnauthorizedAccessException)
            {
                Console.Error.WriteLine("Error: Access denied");
                return ErrorAccessDenied;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during encryption: {ex.Message}");
                return ErrorEncryption;
            }
        }

        /// <summary>
        /// Encrypts or decrypts a file using XOR encryption
        /// </summary>
        /// <param name="sourceFile">Source file path</param>
        /// <param name="targetFile">Target file path</param>
        public static void EncryptFile(string sourceFile, string targetFile)
        {
            string? targetDirectory = Path.GetDirectoryName(targetFile);
            if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            using FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            using FileStream targetStream = new FileStream(targetFile, FileMode.Create, FileAccess.Write);

            byte[] buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    buffer[i] = (byte)(buffer[i] ^ EncryptionKey[i % EncryptionKey.Length]);
                }

                targetStream.Write(buffer, 0, bytesRead);
            }
        }
    }
}