using System;
using System.Linq;
using System.Numerics;
using System.Globalization;

namespace asymCrypto_2
{
  
    class Program
    {
        static string HexToBin(string hexString)
        {
            string binString = "";
            for (int i = 0; i < hexString.Length; i++)
            {
                if (hexString[i] == '0') { binString += "0000";}
                if (hexString[i] == '1') { binString += "0001"; }
                if (hexString[i] == '2') { binString += "0010"; }
                if (hexString[i] == '3') { binString += "0011"; }
                if (hexString[i] == '4') { binString += "0100"; }
                if (hexString[i] == '5') { binString += "0101"; }
                if (hexString[i] == '6') { binString += "0110"; }
                if (hexString[i] == '7') { binString += "0111"; }
                if (hexString[i] == '8') { binString += "1000"; }
                if (hexString[i] == '9') { binString += "1001"; }
                if (hexString[i] == 'A') { binString += "1010"; }
                if (hexString[i] == 'B') { binString += "1011"; }
                if (hexString[i] == 'C') { binString += "1100"; }
                if (hexString[i] == 'D') { binString += "1101"; }
                if (hexString[i] == 'E') { binString += "1110"; }
                if (hexString[i] == 'F') { binString += "1111"; }
            }

            return binString;
        }

        static BigInteger ParseHex(string hexStr)
        {
            string binary = HexToBin(hexStr);
            BigInteger res = 0;
            foreach (char c in binary)
            {
                res <<= 1;
                res += c == '1' ? 1 : 0;
            }

            return res;
        }
        //-------------------------------------------------------- M I L L E R  -  R A B I N    T E S T ------------------------------------------------------------------------------------
        static int pq_Length = 32;
        static BigInteger TWO = new BigInteger(2);
        static BigInteger GenerateBigInteger(BigInteger max)
        {
            Random rnd = new Random();
            byte[] maxBytes = max.ToByteArray(true, false);
            byte[] seedBytes = new byte[maxBytes.Length];

            rnd.NextBytes(seedBytes);
            seedBytes[seedBytes.Length - 1] &= (byte)0x7F;
            var seed = new BigInteger(seedBytes);

            while (seed > max || seed < TWO)
            {
                rnd.NextBytes(seedBytes);
                seedBytes[seedBytes.Length - 1] &= (byte)0x7F;
                seed = new BigInteger(seedBytes);
            }

            return seed;
        }
        static bool MillerRabinTest(BigInteger num, int k = 30)//k минимум 30 
        {
            if(num == TWO || num == new BigInteger(3))
            {
                return true;
            }
            if(num < TWO || num % TWO == BigInteger.Zero)
            {
                return false;
            }

            BigInteger d = num - BigInteger.One;
            int s = 0;

            while(d % TWO == BigInteger.Zero) // num - 1  = 2^s * d
            {
                d = d / TWO;
                s++;
            }
            
            for (int i = 0; i < k; i++)
            {
                var a = GenerateBigInteger(num - TWO);
                var x = BigInteger.ModPow(a, d, num);
                if (x == BigInteger.One || x == num - BigInteger.One)
                {
                    continue;
                }
                for(int j=0; j < s; j++)
                {
                    x = BigInteger.ModPow(x, TWO, num);
                    if ( x == BigInteger.One)
                    {
                        return false;
                    }
                    if(x == num - BigInteger.One)
                    {
                        break;
                    }
                }
                if(x != num - BigInteger.One)
                {
                    return false;
                }
            }

            return true;
        }
        //--------------------------------------------------------- P R I M E   N U M B E R S   G E N E R A T I O N----------------------------------------------------------------------------------------
        static byte[] GenerateRandomByteSeed(int size)//функция генерации начального заполнения ( сейчас тут встроенный но можно любой)
        {
            Random rnd = new Random();
            byte[] seed = new byte[size];
            byte[] zeros = new byte[size];
            Array.Fill(zeros, (byte)0);
            rnd.NextBytes(seed);
            if (seed.SequenceEqual(zeros))
            {
                seed[seed.Length - 1] = (byte)1;
            }
            return seed;
        }
        static BigInteger GeneratePrime(int byteLength)
        {
            var bytes = GenerateRandomByteSeed(byteLength);
            var num = BigInteger.Abs(new BigInteger(bytes));

            while (!MillerRabinTest(num))
            {
                bytes = GenerateRandomByteSeed(byteLength);
                num = BigInteger.Abs(new BigInteger(bytes));
            }
            return num;
        }
        static Tuple<BigInteger,BigInteger> GeneratePrimePair(int byteLength)
        {
            BigInteger p = GeneratePrime(byteLength);
            BigInteger q = GeneratePrime(byteLength);
            while(p.ToByteArray().Length > byteLength || q.ToByteArray().Length > byteLength)
            {
                p = GeneratePrime(byteLength);
                q = GeneratePrime(byteLength);
            }
            while (BigInteger.Compare(p, q) == 0)
            {
                q = GeneratePrime(byteLength);
            }
            return Tuple.Create(p, q);
        }

        //--------------------------------------------------------------------   R S A   --------------------------------------------------------------------------------------

        static BigInteger Inverse(BigInteger num, BigInteger mod)
        {
            BigInteger q, r, t, u1 = BigInteger.One, u2 = BigInteger.Zero, v1 = BigInteger.Zero, v2 = BigInteger.One,
                        a = num, b = mod;
            while (b != BigInteger.Zero)
            {
                q = a / b;
                r = a % b;
                a = b; b = r;
                t = u2;
                u2 = u1 - q * u2;
                u1 = t;
                t = v2;
                v2 = v1 - q * v2;
                v1 = t;
            }
            if (u1 < BigInteger.Zero)
            {
                u1 += mod;
            }
            return u1;
        }

        static Tuple<Tuple<BigInteger, BigInteger, BigInteger>, Tuple<BigInteger, BigInteger>> GenerateKeyPair()
        {
            BigInteger p, q, d, n, e;
            var pair = GeneratePrimePair(pq_Length);// надо где -то учитывать что p*q <= p1*q1
            p = pair.Item1;
            q = pair.Item2;
            n = p * q;
            e = new BigInteger(Math.Pow(2, 16) + 1);
            BigInteger phi_n = (p - BigInteger.One) * (q - BigInteger.One);
            d = Inverse(e, phi_n);

            var publicKey = Tuple.Create(n, e);
            var privateKey = Tuple.Create(d, p, q);

            return Tuple.Create(privateKey, publicKey);
        }

        static BigInteger Encrypt(BigInteger M, BigInteger n, BigInteger e)
            => BigInteger.ModPow(M, e, n);

        static BigInteger Decrypt(BigInteger C, BigInteger d, BigInteger n)
            => BigInteger.ModPow(C, d, n);

        static BigInteger Sign(BigInteger M, BigInteger n, BigInteger d)
            => BigInteger.ModPow(M, d, n);

        static bool Verify(BigInteger M, BigInteger S, BigInteger n, BigInteger e) // verified => true
            => BigInteger.Compare(M, BigInteger.ModPow(S, e, n)) == 0;

        static Tuple<BigInteger, BigInteger> SendKey(BigInteger e1, BigInteger n1, BigInteger n, BigInteger d, BigInteger k)
        {
            BigInteger S = BigInteger.ModPow(k, d, n);
            BigInteger k1 = BigInteger.ModPow(k, e1, n1);
            BigInteger S1 = BigInteger.ModPow(S, e1, n1);
            Console.WriteLine("S = " + S.ToString("X"));
            return Tuple.Create(k1, S1);
        }

        static bool RecieveKey(BigInteger d1, BigInteger n1, BigInteger k1, BigInteger S1, BigInteger e, BigInteger n)
        {
            BigInteger k = BigInteger.ModPow(k1, d1, n1);
            BigInteger S = BigInteger.ModPow(S1, d1, n1);
            Console.WriteLine("k = " + k.ToString("X"));
            Console.WriteLine("S = " + S.ToString("X"));
            
            return Verify(k, S, n, e);
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------

        static void Main(string[] args)
        {
            var m = ParseHex("BCCAE63B06355BCDA56EE08F4127DFF5E38C6CAE9B3BEB2CCD5887C5D6DD1E15");
            Console.WriteLine(m.ToString("X"));
            Console.ReadKey();
        }
    }
}

