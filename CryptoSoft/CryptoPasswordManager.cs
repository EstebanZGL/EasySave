using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CryptoSoft
{
    /// <summary>
    /// Classe utilitaire pour gérer le mot de passe de chiffrement de CryptoSoft
    /// Cette classe peut être utilisée par EasySave pour accéder et modifier le mot de passe
    /// </summary>
    public static class CryptoPasswordManager
    {
        // Fichier contenant la clé de chiffrement hashée
        private const string KeyFilePath = "cryptosoft_key.dat";

        /// <summary>
        /// Récupère le mot de passe actuel (version hashée)
        /// </summary>
        /// <returns>Le mot de passe hashé</returns>
        public static string GetHashedPassword()
        {
            try
            {
                if (File.Exists(KeyFilePath))
                {
                    return File.ReadAllText(KeyFilePath);
                }
                else
                {
                    // Si le fichier n'existe pas, retourner une chaîne vide
                    return string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Vérifie si le mot de passe fourni correspond au mot de passe stocké
        /// </summary>
        /// <param name="password">Le mot de passe à vérifier</param>
        /// <returns>True si le mot de passe correspond, sinon False</returns>
        public static bool VerifyPassword(string password)
        {
            try
            {
                string storedHash = GetHashedPassword();
                if (string.IsNullOrEmpty(storedHash))
                {
                    return false;
                }

                string inputHash = HashPassword(password);
                return storedHash.Equals(inputHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Définit un nouveau mot de passe pour le chiffrement
        /// </summary>
        /// <param name="newPassword">Le nouveau mot de passe</param>
        /// <returns>True si le mot de passe a été changé avec succès, sinon False</returns>
        public static bool SetPassword(string newPassword)
        {
            try
            {
                string hashedPassword = HashPassword(newPassword);
                File.WriteAllText(KeyFilePath, hashedPassword);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Hashe un mot de passe pour le stockage sécurisé
        /// </summary>
        /// <param name="password">Le mot de passe en clair</param>
        /// <returns>Le mot de passe hashé</returns>
        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Convertir le mot de passe en bytes
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                
                // Calculer le hash
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                
                // Convertir le hash en chaîne hexadécimale
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                
                return builder.ToString();
            }
        }
    }
}