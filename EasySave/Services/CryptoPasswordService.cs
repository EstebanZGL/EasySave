using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EasySave.Services
{
    public class CryptoPasswordService
    {
        private static readonly string KeyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cryptosoft_key.dat");
        private const string DefaultPassword = "EasySave2026";
        
        // Shared symmetric key between applications
        private static readonly byte[] SymmetricKey = new byte[] 
        { 
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 
            0x76, 0x65, 0x64, 0x65, 0x76, 0x2c, 0x20, 0x52 
        };

        public bool PasswordFileExists()
        {
            return File.Exists(KeyFilePath);
        }

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
                    // Create with default password if file doesn't exist
                    SetPassword(DefaultPassword);
                    return DefaultPassword;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CryptoPasswordService] Error getting password: {ex.Message}");
                return DefaultPassword;
            }
        }

        public bool SetPassword(string newPassword)
        {
            try
            {
                string encryptedPassword = EncryptPassword(newPassword);
                File.WriteAllText(KeyFilePath, encryptedPassword);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CryptoPasswordService] Error setting password: {ex.Message}");
                return false;
            }
        }
        
        private string EncryptPassword(string password)
        {
            try
            {
                // Create SHA256 hash of the password
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                    
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        builder.Append(hashBytes[i].ToString("x2"));
                    }
                    
                    // Store hash and original password separated by special character
                    // Format: HASH:PASSWORD_ENCRYPTED
                    string hash = builder.ToString();
                    string encryptedPassword = SimpleEncrypt(password, SymmetricKey);
                    
                    return $"{hash}:{encryptedPassword}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CryptoPasswordService] Error encrypting password: {ex.Message}");
                return string.Empty;
            }
        }
        
        private string DecryptPassword(string encryptedData)
        {
            try
            {
                // Separate hash and encrypted password
                string[] parts = encryptedData.Split(':');
                if (parts.Length != 2)
                    return DefaultPassword;
                
                string encryptedPassword = parts[1];
                return SimpleDecrypt(encryptedPassword, SymmetricKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CryptoPasswordService] Error decrypting password: {ex.Message}");
                return DefaultPassword;
            }
        }
        
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