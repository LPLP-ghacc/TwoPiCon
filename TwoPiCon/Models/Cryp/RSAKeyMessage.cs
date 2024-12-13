using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoPiCon.Models.Cryp;

public class RSAKeyMessage
{
    public RSAKeyMessage(string modulus, string exponent)
    {
        Modulus = modulus;
        Exponent = exponent;
    }

    public string Modulus { get; }
    public string Exponent { get; }

    public override string ToString()
    {
        return $"Modulus: {Modulus}\nExponent: {Exponent}";
    }
}
