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
    /// <summary>
    /// CryptoSoft - A simple file encryption utility designed to be called from EasySave.
    /// Version 4.0: Now with a simple GUI and mono-instance support.
    /// </summary>
    public class Program
    {
        // Fichier contenant la clé de chiffrement hashée
        private static readonly string KeyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cryptosoft_key.dat");
        
        // Clé secrète utilisée pour le hachage symétrique (partagée avec EasySave)
        private static readonly byte[] SymmetricKey = new byte[] 
        { 
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 
            0x76, 0x65, 0x64, 0x65, 0x76, 0x2c, 0x20, 0x52 
        };

        // Clé par défaut à utiliser si aucun fichier de clé n'est trouvé
        private const string DefaultPassword = "EasySave2026";

        private const int Success = 0;
        private const int ErrorInvalidArgs = 1;
        private const int ErrorFileNotFound = 2;
        private const int ErrorAccessDenied = 3;
        private const int ErrorEncryption = 4;
        private const int ErrorMutexTimeout = 5;

        // Name of the mutex to ensure single instance
        private const string MutexName = "Local\\CryptoSoft_SingleInstance_Mutex";
        
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
                // Try to create or open the named mutex (Local to session to avoid permission issues)
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
                    // Si le fichier de clé n'existe pas, le créer avec la clé par défaut
                    if (!File.Exists(KeyFilePath))
                    {
                        SaveEncryptionKey(DefaultPassword);
                    }

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
                // Global error handler to prevent silent crashes
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
        /// <param name="password">Optional password to use (uses saved key if null)</param>
        public static void EncryptFile(string sourceFile, string targetFile, string? password = null)
        {
            string? targetDirectory = Path.GetDirectoryName(targetFile);
            if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            // Obtenir la clé de chiffrement à partir du fichier
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

        /// <summary>
        /// Charge la clé de chiffrement depuis le fichier et la déchiffre
        /// </summary>
        /// <returns>Le mot de passe de chiffrement en clair</returns>
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
                    // Si le fichier n'existe pas, créer avec la clé par défaut
                    SaveEncryptionKey(DefaultPassword);
                    return DefaultPassword;
                }
            }
            catch (Exception ex)
            {
                // En cas d'erreur, utiliser la clé par défaut
                Console.Error.WriteLine($"Error loading encryption key: {ex.Message}. Using default.");
                return DefaultPassword;
            }
        }

        /// <summary>
        /// Enregistre la clé de chiffrement dans le fichier après l'avoir chiffrée
        /// </summary>
        /// <param name="password">Le mot de passe à enregistrer</param>
        public static void SaveEncryptionKey(string password)
        {
            try
            {
                string encryptedPassword = EncryptPassword(password);
                File.WriteAllText(KeyFilePath, encryptedPassword);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erreur lors de l'enregistrement de la clé: {ex.Message}");
            }
        }

        /// <summary>
        /// Convertit un mot de passe en clé de chiffrement
        /// </summary>
        /// <param name="password">Le mot de passe en clair</param>
        /// <returns>La clé de chiffrement</returns>
        private static byte[] GetEncryptionKeyFromPassword(string password)
        {
            // Utiliser le hash du mot de passe comme graine pour générer la clé
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                return sha256.ComputeHash(passwordBytes);
            }
        }

        /// <summary>
        /// Chiffre un mot de passe pour le stockage sécurisé
        /// </summary>
        /// <param name="password">Le mot de passe en clair</param>
        /// <returns>Le mot de passe chiffré</returns>
        private static string EncryptPassword(string password)
        {
            try
            {
                // Utiliser SHA256 pour créer un hash du mot de passe
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                    
                    // Convertir le hash en chaîne hexadécimale
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        builder.Append(hashBytes[i].ToString("x2"));
                    }
                    
                    // Stocker le hash et le mot de passe original séparés par un caractère spécial
                    // Le format est: HASH:PASSWORD_ENCRYPTED
                    string hash = builder.ToString();
                    
                    // Chiffrer le mot de passe original avec une clé symétrique
                    string encryptedPassword = SimpleEncrypt(password, SymmetricKey);
                    
                    return $"{hash}:{encryptedPassword}";
                }
            }
            catch (Exception ex)
            {
                // En cas d'erreur, retourner une chaîne vide
                Console.Error.WriteLine($"Error during password encryption: {ex.Message}");
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Déchiffre un mot de passe stocké
        /// </summary>
        /// <param name="encryptedData">Les données chiffrées</param>
        /// <returns>Le mot de passe en clair</returns>
        private static string DecryptPassword(string encryptedData)
        {
            try
            {
                // Séparer le hash et le mot de passe chiffré
                string[] parts = encryptedData.Split(':');
                if (parts.Length != 2)
                    return DefaultPassword;
                
                // Récupérer le mot de passe chiffré
                string encryptedPassword = parts[1];
                
                // Déchiffrer le mot de passe
                return SimpleDecrypt(encryptedPassword, SymmetricKey);
            }
            catch (Exception ex)
            {
                // En cas d'erreur, retourner le mot de passe par défaut
                Console.Error.WriteLine($"Error decrypting password data: {ex.Message}. Using default.");
                return DefaultPassword;
            }
        }
        
        /// <summary>
        /// Chiffrement simple pour le mot de passe
        /// </summary>
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
        
        /// <summary>
        /// Déchiffrement simple pour le mot de passe
        /// </summary>
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