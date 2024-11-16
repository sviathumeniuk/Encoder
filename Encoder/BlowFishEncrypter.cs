using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Modes;

public class BlowfishEncryption
{
    private const int IvSize = 8;
    
    public void EncryptFile(string filePath, string password)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);

        byte[] iv = GenerateIv();
        byte[] encryptedBytes = Encrypt(fileBytes, password, iv);

        byte[] result = new byte[iv.Length + encryptedBytes.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, iv.Length, encryptedBytes.Length);

        File.WriteAllBytes(filePath, result);
    }

    public void DecryptFile(string filePath, string password)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);

        byte[] iv = new byte[IvSize];
        byte[] encryptedData = new byte[fileBytes.Length - IvSize];
        Buffer.BlockCopy(fileBytes, 0, iv, 0, IvSize);
        Buffer.BlockCopy(fileBytes, IvSize, encryptedData, 0, encryptedData.Length);

        byte[] decryptedBytes = Decrypt(encryptedData, password, iv);

        File.WriteAllBytes(filePath, decryptedBytes);
    }

    private byte[] Encrypt(byte[] data, string password, byte[] iv)
    {
        BlowfishEngine engine = new();
        CbcBlockCipher cbc = new(engine);
        BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(cbc);
        cipher.Init(true, new ParametersWithIV(GenerateKeyParameter(password), iv));

        return ProcessData(cipher, data);
    }

    private byte[] Decrypt(byte[] data, string password, byte[] iv)
    {
        BlowfishEngine engine = new();
        CbcBlockCipher cbc = new(engine);
        BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(cbc);
        cipher.Init(false, new ParametersWithIV(GenerateKeyParameter(password), iv));

        return ProcessData(cipher, data);
    }

    private KeyParameter GenerateKeyParameter(string password)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(password);

        if (keyBytes.Length < 16)
        {
            Array.Resize(ref keyBytes, 16);
        }
        else if (keyBytes.Length > 56)
        {
            Array.Resize(ref keyBytes, 56);
        }

        return new KeyParameter(keyBytes);
    }

    private byte[] GenerateIv()
    {
        SecureRandom random = new();
        byte[] iv = new byte[IvSize];
        random.NextBytes(iv);
        return iv;
    }

    private byte[] ProcessData(BufferedBlockCipher cipher, byte[] data)
    {
        byte[] output = new byte[cipher.GetOutputSize(data.Length)];
        int bytesProcessed = cipher.ProcessBytes(data, 0, data.Length, output, 0);
        bytesProcessed += cipher.DoFinal(output, bytesProcessed);
        Array.Resize(ref output, bytesProcessed);
        return output;
    }
}