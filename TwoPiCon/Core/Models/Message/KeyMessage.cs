using System.Security.Cryptography;

public class KeyMessage
{
    public KeyMessage(RSAParameters parameters)
    {
        Parameters = parameters;
    }

    public RSAParameters Parameters { get; set; }
}

