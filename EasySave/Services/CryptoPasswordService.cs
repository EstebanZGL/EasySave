using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EasySave.Services
{
    /// <summary>
    /// Service permettant à EasySave de gérer le mot de passe de chiffrement utilisé par CryptoSoft
    /// </summary>
    public class CryptoPasswordService
    {
        // Fichier contenant la clé de chiffrement hashée (même chemin que CryptoSoft)
        private const string KeyFilePath = "cryptosoft_key.dat";
        
        // Mot de passe par défaut à utiliser si aucun fichier n'existe
        private const string DefaultPassword = "EasySave2026";
        
        // Clé secrète utilisée pour le hachage symétrique (partagée entre les applications)
        private static readonly byte[] SymmetricKey = new byte[] 
        { 
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 
            0x76, 0x65, 0x64, 0x65, 0x76, 0x2c, 0x20, 0x52 
        };

        /// <summary>
        /// Vérifie si le fichier de mot de passe existe
        /// </summary>
        /// <returns>True si le fichier existe, sinon False</returns>
        public bool PasswordFileExists()
        {
            return File.Exists(KeyFilePath);
        }

        /// <summary>
        /// Récupère le mot de passe actuel en clair (déchiffré)
        /// </summary>
        /// <returns>Le mot de passe en clair</returns>
        public string GetPassword()
        {
            try
            {
                if (File.Exists(KeyFilePath))
                {
                    string hashedPassword = File.ReadAllText(KeyFilePath);
                    return DecryptPassword(hashedPassword);
                }
                else
                {
                    // Si le fichier n'existe pas, créer avec le mot de passe par défaut
                    SetPassword(DefaultPassword);
                    return DefaultPassword;
                }
            }
            catch (Exception)
            {
                // En cas d'erreur, retourner le mot de passe par défaut
                return DefaultPassword;
            }
        }

        /// <summary>
        /// Définit un nouveau mot de passe pour le chiffrement
        /// </summary>
        /// <param name="newPassword">Le nouveau mot de passe</param>
        /// <returns>True si le mot de passe a été changé avec succès, sinon False</returns>
        public bool SetPassword(string newPassword)
        {
            try
            {
                string encryptedPassword = EncryptPassword(newPassword);
                File.WriteAllText(KeyFilePath, encryptedPassword);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Chiffre un mot de passe pour le stockage sécurisé
        /// </summary>
        /// <param name="password">Le mot de passe en clair</param>
        /// <returns>Le mot de passe chiffré</returns>
        private string EncryptPassword(string password)
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
            catch (Exception)
            {
                // En cas d'erreur, retourner une chaîne vide
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Déchiffre un mot de passe stocké
        /// </summary>
        /// <param name="encryptedData">Les données chiffrées</param>
        /// <returns>Le mot de passe en clair</returns>
        private string DecryptPassword(string encryptedData)
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
            catch (Exception)
            {
                // En cas d'erreur, retourner le mot de passe par défaut
                return DefaultPassword;
            }
        }
        
        /// <summary>
        /// Chiffrement simple pour le mot de passe
        /// </summary>
        private string SimpleEncrypt(string text, byte[] key)
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
        private string SimpleDecrypt(string encryptedText, byte[] key)
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