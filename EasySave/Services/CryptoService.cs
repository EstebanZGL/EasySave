using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using EasySave.ViewModels;

namespace EasySave.Services
{
    public class CryptoService
    {
        private readonly SettingsViewModel _settings;

        public CryptoService(SettingsViewModel settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Determines if a file should be encrypted based on its extension
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if the file should be encrypted, false otherwise</returns>
        public bool ShouldEncrypt(string filePath)
        {
            return _settings.ShouldEncryptFile(filePath);
        }

        /// <summary>
        /// Encrypts a file using CryptoSoft
        /// </summary>
        /// <param name="sourceFile">Path to the source file</param>
        /// <param name="targetFile">Path where the encrypted file should be saved</param>
        /// <returns>Time taken to encrypt in milliseconds, or -1 if encryption failed</returns>
        public async Task<long> EncryptFileAsync(string sourceFile, string targetFile)
        {
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException("Source file not found", sourceFile);

            if (string.IsNullOrEmpty(_settings.CryptoSoftPath) || !File.Exists(_settings.CryptoSoftPath))
                throw new FileNotFoundException("CryptoSoft executable not found", _settings.CryptoSoftPath);

            // Ensure the target directory exists
            string targetDir = Path.GetDirectoryName(targetFile);
            if (!string.IsNullOrEmpty(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // First copy the file to the destination
            File.Copy(sourceFile, targetFile, true);

            // Then encrypt it in place
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Create process to run CryptoSoft
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _settings.CryptoSoftPath,
                        Arguments = $"\"{targetFile}\"", // Pass the target file path as an argument
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                // Start the process
                process.Start();
                
                // Wait for the process to exit asynchronously
                await process.WaitForExitAsync();
                
                // Check if the process exited successfully
                if (process.ExitCode != 0)
                {
                    Debug.WriteLine($"CryptoSoft exited with code {process.ExitCode}");
                    return -1;
                }

                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error encrypting file: {ex.Message}");
                return -1;
            }
        }
    }
}