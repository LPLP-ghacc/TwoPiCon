using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TwoPiCon.Core;

public enum СryptographyPower
{
    AtomicKeys,
    High,
    Medium,
    Low
}

public class Сryptographer
{
}

public class RSAEncryption : IDisposable
{
    private RSA _rsa;
    private RSAParameters _privateKey;
    private RSAParameters _publicKey;

    public RSAEncryption(int keySize = 2048)
    {
        _rsa = RSA.Create(keySize);
        _privateKey = _rsa.ExportParameters(true);
        _publicKey = _rsa.ExportParameters(false);
    }

    public byte[] Encrypt(string data, RSAParameters publicKey)
    {
        using (RSA rsaEncryptor = RSA.Create())
        {
            rsaEncryptor.ImportParameters(publicKey);
            return rsaEncryptor.Encrypt(Encoding.UTF8.GetBytes(data), RSAEncryptionPadding.Pkcs1);
        }
    }

    public string Decrypt(byte[] data)
    {
        try
        {
            lock (_rsa)
            {
                _rsa.ImportParameters(_privateKey);
                byte[] decryptedData = _rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
                return Encoding.UTF8.GetString(decryptedData);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error decrypting message: {ex.Message}");
            throw;
        }
    }

    public RSAParameters GetPublicKey()
    {
        return _publicKey;
    }

    public void Dispose()
    {
        _rsa.Dispose();
    }
}