using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CryptoSoft
{
    /// <summary>
    /// CryptoSoft - A simple file encryption utility designed to be called from EasySave.
    /// Version 3.0: Now with mono-instance support using a system-wide mutex.
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

        public static int Main(string[] args)
        {
            bool mutexCreated = false;
            // Initialiser le mutex à une valeur non nulle
            Mutex? mutex = null;

            try
            {
                // Try to create or open the named mutex
                mutex = new Mutex(false, MutexName, out mutexCreated);
                
                Console.WriteLine("CryptoSoft v3.0 - Mono-instance encryption utility");
                
                if (args.Length != 2)
                {
                    Console.Error.WriteLine("Usage: CryptoSoft <source_file> <target_file>");
                    return ErrorInvalidArgs;
                }

                string sourceFile = args[0];
                string targetFile = args[1];

                if (!File.Exists(sourceFile))
                {
                    Console.Error.WriteLine($"Error: Source file not found: {sourceFile}");
                    return ErrorFileNotFound;
                }

                Console.WriteLine($"Waiting to acquire encryption lock...");
                
                // Try to acquire the mutex with a timeout
                if (mutex != null && !mutex.WaitOne(MutexTimeout))
                {
                    Console.Error.WriteLine("Error: Timeout waiting for encryption lock. Another encryption process is taking too long.");
                    return ErrorMutexTimeout;
                }
                
                Console.WriteLine($"Lock acquired. Encrypting file: {Path.GetFileName(sourceFile)}");

                try
                {
                    // Perform the encryption
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    EncryptFile(sourceFile, targetFile);
                    stopwatch.Stop();

                    Console.WriteLine(stopwatch.ElapsedMilliseconds);
                    return Success;
                }
                finally
                {
                    // Always release the mutex when done
                    mutex?.ReleaseMutex();
                    Console.WriteLine("Encryption lock released.");
                }
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
            finally
            {
                // Clean up the mutex
                mutex?.Dispose();
            }
        }

        private static void EncryptFile(string sourceFile, string targetFile)
        {
            string targetDirectory = Path.GetDirectoryName(targetFile) ?? string.Empty;
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