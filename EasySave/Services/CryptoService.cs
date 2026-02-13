using System;
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
        /// Determines if a file should be encrypted based on its extension.
        /// </summary>
        public bool ShouldEncrypt(string filePath)
        {
            return _settings.ShouldEncryptFile(filePath);
        }

        /// <summary>
        /// Encrypts a file using CryptoSoft.
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

            string targetDir = Path.GetDirectoryName(targetFile) ?? string.Empty;
            if (!string.IsNullOrEmpty(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _settings.CryptoSoftPath,
                        Arguments = $"\"{sourceFile}\" \"{targetFile}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                string stdOut = await process.StandardOutput.ReadToEndAsync();
                string stdErr = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    Debug.WriteLine($"CryptoSoft failed with code {process.ExitCode}. Error: {stdErr}");
                    return -1;
                }

                if (long.TryParse(stdOut.Trim(), out long encryptionTime) && encryptionTime >= 0)
                {
                    return encryptionTime;
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
