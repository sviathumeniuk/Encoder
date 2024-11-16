using System.IO;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using System.Text;

public class AesEncryption
{
    public void EncryptFile(string filePath, string password)
    {
        try
        {
            byte[] fileBytes;
            using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, fileBytes.Length);
            }
            byte[] encryptedBytes = Encrypt(fileBytes, password);
            bool sizeChanged = encryptedBytes.Length != fileBytes.Length;
            if (sizeChanged)
            {
                MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.ReadWrite);
                mmf.Dispose();
                using FileStream fs = new(filePath, FileMode.Open, FileAccess.Write);
                fs.SetLength(encryptedBytes.Length);
            }
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, encryptedBytes.Length, MemoryMappedFileAccess.ReadWrite))
            {
                using MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(0, encryptedBytes.Length, MemoryMappedFileAccess.Write);
                accessor.WriteArray(0, encryptedBytes, 0, encryptedBytes.Length);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to encrypt file {filePath}: {ex.Message}", ex);
        }
    }

    public void DecryptFile(string filePath, string password)
    {
        try
        {
            byte[] encryptedBytes;
            using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read))
            {
                encryptedBytes = new byte[fs.Length];
                fs.Read(encryptedBytes, 0, encryptedBytes.Length);
            }
            byte[] decryptedBytes = Decrypt(encryptedBytes, password);
            bool sizeChanged = decryptedBytes.Length != encryptedBytes.Length;
            if (sizeChanged)
            {
                MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.ReadWrite);
                mmf.Dispose();
                using FileStream fs = new(filePath, FileMode.Open, FileAccess.Write);
                fs.SetLength(decryptedBytes.Length);
            }
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, decryptedBytes.Length, MemoryMappedFileAccess.ReadWrite))
            {
                using MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(0, decryptedBytes.Length, MemoryMappedFileAccess.Write);
                accessor.WriteArray(0, decryptedBytes, 0, decryptedBytes.Length);
            }
        }
        catch (CryptographicException)
        {
            throw new Exception("Decryption failed. The password may be incorrect or the data is corrupted.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to decrypt file {filePath}: {ex.Message}", ex);
        }
    }

    private byte[] Encrypt(byte[] data, string password)
    {
        try
        {
            using Aes aes = Aes.Create();
            byte[] key = CreateKey(password, aes.KeySize / 8);
            aes.Key = key;
            aes.GenerateIV();
            using MemoryStream ms = new();
            ms.Write(aes.IV, 0, aes.IV.Length);
            using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
            }
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            throw new Exception($"Encryption failed: {ex.Message}", ex);
        }
    }

    private byte[] Decrypt(byte[] data, string password)
    {
        try
        {
            using Aes aes = Aes.Create();
            byte[] key = CreateKey(password, aes.KeySize / 8);
            aes.Key = key;
            int ivLength = aes.BlockSize / 8;
            byte[] iv = new byte[ivLength];
            Array.Copy(data, 0, iv, 0, iv.Length);
            aes.IV = iv;
            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(data, iv.Length, data.Length - iv.Length);
                cs.FlushFinalBlock();
            }
            return ms.ToArray();
        }
        catch (CryptographicException)
        {
            throw new Exception("Decryption failed. The password may be incorrect or the data is corrupted.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Decryption failed: {ex.Message}", ex);
        }
    }

    private byte[] CreateKey(string password, int keyBytes)
    {
        byte[] salt = Encoding.UTF8.GetBytes("additional_defense");
        using var keyDerivationFunction = new Rfc2898DeriveBytes(password, salt, 1000);
        return keyDerivationFunction.GetBytes(keyBytes);
    }
}