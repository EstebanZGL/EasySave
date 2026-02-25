using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace CryptoSoft
{
    // File encryption utility for EasySave with GUI and single-instance support
    public class Program
    {
        // Path to the encryption key file
        private static readonly string KeyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cryptosoft_key.dat");
        
        // Symmetric key shared with EasySave
        private static readonly byte[] SymmetricKey = new byte[] 
        { 
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 
            0x76, 0x65, 0x64, 0x65, 0x76, 0x2c, 0x20, 0x52 
        };

        private const string DefaultPassword = "EasySave2026";

        private const int Success = 0;
        private const int ErrorInvalidArgs = 1;
        private const int ErrorFileNotFound = 2;
        private const int ErrorAccessDenied = 3;
        private const int ErrorEncryption = 4;
        private const int ErrorMutexTimeout = 5;

        // Mutex to ensure single instance
        private const string MutexName = "Local\\CryptoSoft_SingleInstance_Mutex";
        private const int MutexTimeout = 30000; // 30 seconds
        private static Mutex? _mutex;
        private static bool _mutexCreated;

        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                _mutex = new Mutex(false, MutexName, out _mutexCreated);
                
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
                    if (!File.Exists(KeyFilePath))
                    {
                        SaveEncryptionKey(DefaultPassword);
                    }

                    if (args.Length == 2)
                    {
                        return RunCliMode(args);
                    }
                    
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new CryptoSoftForm());
                    
                    return Success;
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                if (args.Length == 0 || !Console.IsOutputRedirected)
                {
                    MessageBox.Show($"Critical error starting CryptoSoft:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "CryptoSoft Fatal Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
                if (args.Length > 0)
                {
                    Console.Error.WriteLine($"Error: {ex.Message}");
                }
                return ErrorEncryption;
            }
            finally
            {
                _mutex?.Dispose();
            }
        }

        // Command line mode for EasySave compatibility
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

        // Encrypts/decrypts a file using XOR
        public static void EncryptFile(string sourceFile, string targetFile, string? password = null)
        {
            string? targetDirectory = Path.GetDirectoryName(targetFile);
            if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            byte[] encryptionKey = GetEncryptionKeyFromPassword(password ?? LoadEncryptionKey());

            using FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            using FileStream targetStream = new FileStream(targetFile, FileMode.Create, FileAccess.Write);

            byte[] buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    buffer[i] = (byte)(buffer[i] ^ encryptionKey[i % encryptionKey.Length]);
                }

                targetStream.Write(buffer, 0, bytesRead);
            }
        }

        // Loads the key from file
        public static string LoadEncryptionKey()
        {
            try
            {
                if (File.Exists(KeyFilePath))
                {
                    string encryptedData = File.ReadAllText(KeyFilePath);
                    return DecryptPassword(encryptedData);
                }
                else
                {
                    SaveEncryptionKey(DefaultPassword);
                    return DefaultPassword;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error loading encryption key: {ex.Message}. Using default.");
                return DefaultPassword;
            }
        }

        // Saves the key to file
        public static void SaveEncryptionKey(string password)
        {
            try
            {
                string encryptedPassword = EncryptPassword(password);
                File.WriteAllText(KeyFilePath, encryptedPassword);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving key: {ex.Message}");
            }
        }

        // Generates encryption key from password
        private static byte[] GetEncryptionKeyFromPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                return sha256.ComputeHash(passwordBytes);
            }
        }

        // Encrypts password for storage
        private static string EncryptPassword(string password)
        {
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                    
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        builder.Append(hashBytes[i].ToString("x2"));
                    }
                    
                    // Format: HASH:PASSWORD_ENCRYPTED
                    string hash = builder.ToString();
                    string encryptedPassword = SimpleEncrypt(password, SymmetricKey);
                    
                    return $"{hash}:{encryptedPassword}";
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during password encryption: {ex.Message}");
                return string.Empty;
            }
        }
        
        // Decrypts stored password
        private static string DecryptPassword(string encryptedData)
        {
            try
            {
                string[] parts = encryptedData.Split(':');
                if (parts.Length != 2)
                    return DefaultPassword;
                
                string encryptedPassword = parts[1];
                return SimpleDecrypt(encryptedPassword, SymmetricKey);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error decrypting password data: {ex.Message}. Using default.");
                return DefaultPassword;
            }
        }
        
        // Simple XOR encryption
        private static string SimpleEncrypt(string text, byte[] key)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            byte[] result = new byte[textBytes.Length];
            
            for (int i = 0; i < textBytes.Length; i++)
            {
                result[i] = (byte)(textBytes[i] ^ key[i % key.Length]);
            }
            
            return Convert.ToBase64String(result);
        }
        
        // Simple XOR decryption
        private static string SimpleDecrypt(string encryptedText, byte[] key)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] result = new byte[encryptedBytes.Length];
            
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                result[i] = (byte)(encryptedBytes[i] ^ key[i % key.Length]);
            }
            
            return Encoding.UTF8.GetString(result);
        }
    }
}