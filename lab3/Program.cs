﻿using System;
using System.Linq;
using System.Numerics;
using System.Globalization;

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
            BigInteger t = new BigInteger(1);
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
            if (result == 1)
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
            BigInteger s1 = BigInteger.ModPow(y, (p + 1) / 4, p);
            BigInteger s2 = BigInteger.ModPow(y, (q + 1) / 4, q);
            var temp = GCD_Ext(p, q);
            BigInteger x1 = (temp.Item1 * p * s1) + (temp.Item2 * q * s2);
            BigInteger x2 = (temp.Item1 * p * s1) - (temp.Item2 * q * s2);
            BigInteger x3 = -(temp.Item1 * p * s1) + (temp.Item2 * q * s2);
            BigInteger x4 = -(temp.Item1 * p * s1) - (temp.Item2 * q * s2);
            return Tuple.Create(x1, x2, x3, x4);
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
            if (num == TWO || num == new BigInteger(3))
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

            while (!MillerRabinTest(num))
            {
                bytes = GenerateRandomByteSeed(byteLength);
                num = BigInteger.Abs(new BigInteger(bytes));
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
            var l = n.ToByteArray().Length;
            if (l - 10 < m.ToByteArray().Length)
            {
                Console.WriteLine("message is too big for this n");
                return BigInteger.Zero;
            }
            else
            {
                var x = new BigInteger(255 * Math.Pow(2, 8 * (l - 8))) + m * new BigInteger(Math.Pow(2, 64)) + r;
                return x;
            }
        }

        static BigInteger DeformatMessage(BigInteger x, BigInteger n)
        {
            var l = n.ToByteArray().Length;
            BigInteger m = (x - r - new BigInteger(255 * Math.Pow(2, 8 * (l - 8)))) / new BigInteger(Math.Pow(2, 64));
            return m;
        }

        static Tuple<BigInteger, BigInteger, BigInteger> Encrypt(BigInteger x, BigInteger b, BigInteger n)
        {
            var y = (x * (x + b)) % n;
            var c1 = ((x + b / 2) % n) % 2;
            var c2 = IversonSymbol(x + b / 2, n);
            return Tuple.Create(y, c1, c2);
        }

        static void Main(string[] args)
        {
            /*var keys = GenerateKey(32, 32);
            var message = "A7A72D350B6E2391";
            var formated = FormatMessage(ParseHex(message), keys.Item4);
            var unformated = DeformatMessage(formated, keys.Item4);
            Console.WriteLine(formated.ToString("X"));
            Console.WriteLine(unformated.ToString("X"));*/


            Console.ReadKey();
        }

    }
}
