using System.Security.Cryptography;
using System.Text;

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

    public static byte[] Encrypt(string data, RSAParameters publicKey)
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
        catch
        {
            return string.Empty;
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

