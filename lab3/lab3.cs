using System;
using System.Numerics;
using System.Linq;

namespace Asym_Crypto_Lab_3
{
    class Program
    {
        static BigInteger TWO = new BigInteger(2);
        static BigInteger THREE = new BigInteger(3);
        static BigInteger FOUR = new BigInteger(4);
        static BigInteger r = GeneratePrime(8);

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

        static Tuple<BigInteger, BigInteger> GCD_Ext(BigInteger num1, BigInteger num2)
        {
            BigInteger q, r, t, u1 = BigInteger.One, u2 = BigInteger.Zero, v1 = BigInteger.Zero, v2 = BigInteger.One,
                        a = num1, b = num2;
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

            return Tuple.Create(u1, v1);
        }

        static BigInteger JacobiSymbol(BigInteger a, BigInteger n)
        {
            BigInteger r, temp;
            BigInteger t = BigInteger.One;
            while (a != 0)
            {
                while (a % 2 == 0)
                {
                    a /= 2;
                    r = n % 8;
                    if (r == 3 || r == 5)
                    {
                        t = -t;
                    }
                }
                temp = a;
                a = n;
                n = temp;
                if (a % 4 == n % 4 && n % 4 == 3)
                {
                    t = -t;
                }
                a %= n;

            }
            if (n == 1)
            {
                return t;
            }
            else
            {
                return 0;
            }
        }


        static BigInteger IversonSymbol(BigInteger a, BigInteger b)
        {
            BigInteger result = JacobiSymbol(a, b);
            if (result == BigInteger.One)
            {
                return new BigInteger(1);
            }
            else
            {
                return new BigInteger(0);
            }
        }


        static Tuple<BigInteger, BigInteger, BigInteger, BigInteger> BlumSqrt(BigInteger y, BigInteger p, BigInteger q)
        {
            BigInteger O = BigInteger.Zero;
            BigInteger s1 = BigInteger.ModPow(y, (p + BigInteger.One) / FOUR, p);
            BigInteger s2 = BigInteger.ModPow(y, (q + BigInteger.One) / FOUR, q);
            var n = p * q;
            var uv = GCD_Ext(p, q);
            BigInteger u = uv.Item1;
            BigInteger v = uv.Item2;
            BigInteger x1 = (u * p * s2 + v * q * s1) % n;
            BigInteger x2 = (u * p * s2 - v * q * s1) % n;
            BigInteger x3 = (-u * p * s2 + v * q * s1) % n;
            BigInteger x4 = (-u * p * s2 - v * q * s1) % n;

            return Tuple.Create(x1 > O ? x1 : x1 + n, x2 > O ? x2 : x2 + n, x3 > O ? x3 : x3 + n, x4 > O ? x4 : x4 + n);
        }

        static string HexToBin(string hexString)
        {
            string binString = "";
            for (int i = 0; i < hexString.Length; i++)
            {
                if (hexString[i] == '0') { binString += "0000"; }
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
            var res = BigInteger.Zero;
            foreach (char c in HexToBin(hexStr))
            {
                res <<= 1;
                res += c == '1' ? 1 : 0;
            }

            return res;
        }

        /* Prime numbers generating */
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

        static bool MillerRabinTest(BigInteger num, int k = 30)
        {
            if (num == TWO || num == THREE)
            {
                return true;
            }
            if (num < TWO || num % TWO == BigInteger.Zero)
            {
                return false;
            }

            BigInteger d = num - BigInteger.One;
            int s = 0;

            while (d % TWO == BigInteger.Zero)
            {
                d /= TWO;
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
                for (int j = 0; j < s; j++)
                {
                    x = BigInteger.ModPow(x, TWO, num);
                    if (x == BigInteger.One)
                    {
                        return false;
                    }
                    if (x == num - BigInteger.One)
                    {
                        break;
                    }
                }
                if (x != num - BigInteger.One)
                {
                    return false;
                }
            }

            return true;
        }

        static byte[] GenerateRandomByteSeed(int size)
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

            Console.WriteLine("Generated numbers: ");
            while (!MillerRabinTest(num))
            {
                bytes = GenerateRandomByteSeed(byteLength);
                num = BigInteger.Abs(new BigInteger(bytes));
                Console.WriteLine(num.ToString("X"));
            }
            return num;
        }

        static BigInteger GenerateBlumNumber(int byteLength)
        {
            var num = GeneratePrime(byteLength);
            while ((num - THREE) % FOUR != 0)
            {
                num = GeneratePrime(byteLength);
            }
            return num;
        }

        /* Rabin cryptosystem */
        static Tuple<BigInteger, BigInteger, BigInteger, BigInteger> GenerateKey(int PbyteLength, int QbyteLength)
        {
            var p = GenerateBlumNumber(PbyteLength);
            var q = GenerateBlumNumber(QbyteLength);
            var n = p * q;
            var rand = new Random();
            var randomBytesLength = rand.Next(1, n.ToByteArray().Length);
            var b = GenerateBlumNumber(randomBytesLength);
            return Tuple.Create(p, q, b, n);
        }

        static BigInteger FormatMessage(BigInteger m, BigInteger n)
        {
            int l = 64;
            if (l - 10 < m.ToByteArray().Length)
            {
                Console.WriteLine("message is too big for this n");
                return BigInteger.Zero;
            }
            else
            {
                var x = r + (m << 64) + (new BigInteger(255) << (8 * (l - 2)));
                return x;
            }
        }

        static BigInteger DeformatMessage(BigInteger x, BigInteger n)
        {
            var l = n.ToByteArray().Length;
            var xStr = x.ToString("X");
            var xWithoutR = xStr.Substring(0, xStr.Length - 16);
            return ParseHex((xStr.Length == 128 ? xWithoutR.Substring(4) : xWithoutR.Substring(3)).TrimStart('0'));
        }

        static Tuple<BigInteger, BigInteger, BigInteger> Encrypt(BigInteger x, BigInteger b, BigInteger n)
        {
            var halfOfB = (b * Inverse(2, n)) % n;
            var y = (x * (x + b)) % n;
            var c1 = ((x + halfOfB) % n) % 2;
            var c2 = IversonSymbol(x + halfOfB, n);
            return Tuple.Create(y, c1, c2);
        }

        static BigInteger Decrypt(Tuple<BigInteger, BigInteger, BigInteger> cipherText, Tuple<BigInteger, BigInteger, BigInteger, BigInteger> key)
        {
            var halfOfB = (key.Item3 * Inverse(TWO, key.Item4)) % key.Item4;
            var temp = (cipherText.Item1 + BigInteger.ModPow(halfOfB, TWO, key.Item4)) % key.Item4;
            var sqrts = BlumSqrt(temp, key.Item1, key.Item2);

            BigInteger x = BigInteger.Zero;
            if (sqrts.Item1 % TWO == cipherText.Item2 && IversonSymbol(sqrts.Item1, key.Item4) == cipherText.Item3) { x = sqrts.Item1; }
            if (sqrts.Item2 % TWO == cipherText.Item2 && IversonSymbol(sqrts.Item2, key.Item4) == cipherText.Item3) { x = sqrts.Item2; }
            if (sqrts.Item3 % TWO == cipherText.Item2 && IversonSymbol(sqrts.Item3, key.Item4) == cipherText.Item3) { x = sqrts.Item3; }
            if (sqrts.Item4 % TWO == cipherText.Item2 && IversonSymbol(sqrts.Item4, key.Item4) == cipherText.Item3) { x = sqrts.Item4; }
            x -= halfOfB;
            return DeformatMessage(x, key.Item4);

        }


        static BigInteger Sign(BigInteger m, Tuple<BigInteger, BigInteger, BigInteger, BigInteger> key)
        {
            BigInteger x = FormatMessage(m, key.Item4);
            while (JacobiSymbol(x, key.Item1) != BigInteger.One || JacobiSymbol(x, key.Item2) != BigInteger.One)
            {
                r = GeneratePrime(8);
                x = FormatMessage(m, key.Item4);
            }
            var sqrts = BlumSqrt(x, key.Item1, key.Item2);
            return sqrts.Item1;
        }

        static bool Verify(BigInteger s, BigInteger m, BigInteger n)
           => BigInteger.Compare(DeformatMessage(BigInteger.ModPow(s, TWO, n), n), m) == 0;


        /* Zero Knoledge Protocol*/
        static BigInteger ZKPsendY(BigInteger n, BigInteger x)
            => BigInteger.ModPow(x, FOUR, n);


        static BigInteger ZPKrecieveYsendZ(BigInteger p, BigInteger q, BigInteger y)
        {
            BigInteger z = 0;
            var sqrts = BlumSqrt(y, p, q);
            if (JacobiSymbol(sqrts.Item1, p) == 1 && JacobiSymbol(sqrts.Item1, q) == 1) { z = sqrts.Item1; }
            if (JacobiSymbol(sqrts.Item2, p) == 1 && JacobiSymbol(sqrts.Item2, q) == 1) { z = sqrts.Item2; }
            if (JacobiSymbol(sqrts.Item3, p) == 1 && JacobiSymbol(sqrts.Item3, q) == 1) { z = sqrts.Item3; }
            if (JacobiSymbol(sqrts.Item4, p) == 1 && JacobiSymbol(sqrts.Item4, q) == 1) { z = sqrts.Item4; }
            return z;
        }

        static bool ZPKrecieveZ(BigInteger n, BigInteger z, BigInteger x)
            => BigInteger.Compare(BigInteger.ModPow(x, TWO, n), z) == 0;

        /* Zero Knoledge Protocol Attack*/
        static BigInteger ZKP_Attack_sendY(BigInteger n, BigInteger t)
            => BigInteger.ModPow(t, TWO, n);

        static BigInteger ZKP_Attack_getDivider(BigInteger z, BigInteger n, BigInteger t)
        {
            if (t == z || t == -z)
            {
                Console.WriteLine("Could not find p or q");
                return BigInteger.Zero;
            }
            else
            {
                var pq = BigInteger.GreatestCommonDivisor(t + z, n);
                if (pq == BigInteger.One)
                {
                    Console.WriteLine("Could not find p or q");

                }
                return pq;
            }
        }

        static void Main(string[] args)
        {
            var rndm = new Random();

            var keys = GenerateKey(32, 32);
            Console.WriteLine("p = " + keys.Item1.ToString("X"));
            Console.WriteLine("q = " + keys.Item2.ToString("X"));
            Console.WriteLine("b = " + keys.Item3.ToString("X"));
            Console.WriteLine("n = " + keys.Item4.ToString("X"));

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("==================================================");
                Console.WriteLine();

                Console.WriteLine("What do you want to do?");
                Console.WriteLine("----------");
                Console.WriteLine("Enc - Encryption");
                Console.WriteLine("Dec - Decryption");
                Console.WriteLine("Sign - Sign");
                Console.WriteLine("Ver - Verification");
                Console.WriteLine("A - Zero Knowledge Attack");
                Console.WriteLine("END - End");
                Console.WriteLine("----------");
                Console.Write("Your choice: ");
                string action = Console.ReadLine();

                if (action == "End")
                {
                    break;
                }

                if (action == "Enc")
                {
                    Console.WriteLine("----------");
                    Console.Write("Enter M: ");
                    string M_hex = Console.ReadLine();
                    var M = ParseHex(M_hex);

                    Console.Write("Enter n: ");
                    string n_hex = Console.ReadLine();
                    var n = ParseHex(n_hex);

                    Console.Write("Enter b: ");
                    string b_hex = Console.ReadLine();
                    var b = ParseHex(b_hex);

                    var formated = FormatMessage(M, n);
                    var C = Encrypt(formated, b, n);
                    Console.WriteLine("Y = " + C.Item1.ToString("X"));
                    Console.WriteLine("Parity = " + C.Item2.ToString("X"));
                    Console.WriteLine("Jacobi = " + C.Item3.ToString("X"));
                    Console.WriteLine("----------");
                }

                if (action == "Dec")
                {
                    Console.WriteLine("----------");

                    Console.Write("Enter cipherText: ");
                    string c_hex = Console.ReadLine();
                    var c = ParseHex(c_hex);

                    Console.Write("Enter parity: ");
                    string parity_hex = Console.ReadLine();
                    var parity = ParseHex(parity_hex);

                    Console.Write("Enter jacobi: ");
                    string jacobi_hex = Console.ReadLine();
                    var jacobi = ParseHex(jacobi_hex);

                    var cipher = Tuple.Create(c, parity, jacobi);

                    var dec = Decrypt(cipher, keys);
                    Console.WriteLine("Decrypted: " + dec.ToString("X"));
                    Console.WriteLine("----------");
                }

                if (action == "Sign")
                {
                    Console.WriteLine("----------");
                    Console.Write("Enter M: ");
                    string M_hex = Console.ReadLine();
                    var M = ParseHex(M_hex);

                    var s = Sign(M, keys);
                    Console.WriteLine("S = " + s.ToString("X"));
                    Console.WriteLine("----------");
                }

                if (action == "Ver")
                {
                    Console.WriteLine("----------");
                    Console.Write("Enter M: ");
                    string M_hex = Console.ReadLine();
                    var M = ParseHex(M_hex);

                    Console.Write("Enter S: ");
                    string S_hex = Console.ReadLine();
                    var S = ParseHex(S_hex);

                    Console.Write("Enter n: ");
                    string n_hex = Console.ReadLine();
                    var n = ParseHex(n_hex);

                    var ver = Verify(S, M, n);
                    Console.WriteLine("Verification result :" + ver);
                    Console.WriteLine("----------");
                }

                if (action == "A")
                {
                    while (true)
                    {
                        int tByteLength = rndm.Next(1, 64);
                        BigInteger t = GeneratePrime(tByteLength);

                        Console.Write("Enter n:");
                        string n_hex = Console.ReadLine();
                        var n = ParseHex(n_hex);

                        var fauxY = ZKP_Attack_sendY(n, t);
                        Console.WriteLine("y  =  " + fauxY.ToString("X"));

                        Console.Write("Enter z:");
                        string z_hex = Console.ReadLine();
                        var z = ParseHex(z_hex);

                        var foundP = ZKP_Attack_getDivider(z, n, t);
                        if (foundP != BigInteger.One)
                        {
                            Console.WriteLine("P  =  " + foundP.ToString("X"));
                            var foundQ = n / foundP;
                            Console.WriteLine("Q  =  " + foundQ.ToString("X"));
                            Console.Write("computed n = ");
                            Console.Write((foundP * foundQ).ToString("X"));
                            Console.WriteLine(foundQ * foundP == n);
                            break;
                        }
                    }

                }
            }

            Console.ReadKey();
        }
    }
}