using System;
using System.Diagnostics;
using System.IO;

namespace CryptoSoft
{
    /// <summary>
    /// CryptoSoft - A simple file encryption utility designed to be called from EasySave.
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

        public static int Main(string[] args)
        {
            try
            {
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
