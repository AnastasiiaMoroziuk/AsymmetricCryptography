using System;
using System.Linq;
using System.Numerics;
using System.Globalization;

namespace Asym_Crypto_Lab_2 {

    class Program {

        static string HexToBin(string hexString) {
            string binString = "";

            for (int i = 0; i < hexString.Length; i++) {
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

        static BigInteger ParseHex(string hexStr) {
            string binaryStr = HexToBin(hexStr);
            var res = BigInteger.Zero;
            foreach (char c in binaryStr) {
                res <<= 1;
                res += c == '1' ? 1 : 0;
            }

            return res;
        }

        static string DecToHex(BigInteger num) {
            return num.ToString("X");
        }

        /* Miller-Rabin Test */
        static BigInteger TWO = new BigInteger(2);

        static BigInteger GenerateBigInteger(BigInteger max) {
            Random rnd = new Random();
            byte[] maxBytes = max.ToByteArray(true, false);
            byte[] seedBytes = new byte[maxBytes.Length];

            rnd.NextBytes(seedBytes);
            seedBytes[seedBytes.Length - 1] &= (byte)0x7F;
            var seed = new BigInteger(seedBytes);

            while (seed > max || seed < TWO) {
                rnd.NextBytes(seedBytes);
                seedBytes[seedBytes.Length - 1] &= (byte)0x7F;
                seed = new BigInteger(seedBytes);
            }

            return seed;
        }

        static bool MillerRabinTest(BigInteger num, int k = 30) {
            if (num == TWO || num == new BigInteger(3)) {
                return true;
            }
            if (num < TWO || num % TWO == BigInteger.Zero) {
                return false;
            }

            BigInteger d = num - BigInteger.One;
            int s = 0;

            while (d % TWO == BigInteger.Zero){
                d = d / TWO;
                s++;
            }

            for (int i = 0; i < k; i++) {
                var a = GenerateBigInteger(num - TWO);
                var x = BigInteger.ModPow(a, d, num);
                if (x == BigInteger.One || x == num - BigInteger.One) {
                    continue;
                }
                for (int j = 0; j < s; j++) {
                    x = BigInteger.ModPow(x, TWO, num);
                    if (x == BigInteger.One) {
                        return false;
                    }
                    if (x == num - BigInteger.One) {
                        break;
                    }
                }
                if (x != num - BigInteger.One) {
                    return false;
                }
            }

            return true;
        }

        /* Prime numbers generator */
        static byte[] GenerateRandomByteSeed(int size) {
            Random rnd = new Random();
            byte[] seed = new byte[size];
            byte[] zeros = new byte[size];
            Array.Fill(zeros, (byte)0);
            rnd.NextBytes(seed);
            if (seed.SequenceEqual(zeros)) {
                seed[seed.Length - 1] = (byte)1;
            }
            return seed;
        }

        static BigInteger GeneratePrime(int byteLength) {
            var bytes = GenerateRandomByteSeed(byteLength);
            var num = BigInteger.Abs(new BigInteger(bytes));

            while (!MillerRabinTest(num)) {
                bytes = GenerateRandomByteSeed(byteLength);
                num = BigInteger.Abs(new BigInteger(bytes));
            }
            return num;
        }

        static Tuple<BigInteger, BigInteger> GeneratePrimePair(int byteLength) {
            BigInteger p = GeneratePrime(byteLength);
            BigInteger q = GeneratePrime(byteLength);
            while (p.ToByteArray().Length > byteLength || q.ToByteArray().Length > byteLength) {
                p = GeneratePrime(byteLength);
                q = GeneratePrime(byteLength);
            }
            while (BigInteger.Compare(p, q) == 0) {
                q = GeneratePrime(byteLength);
            }
            return Tuple.Create(p, q);
        }

        /* RSA */
        static BigInteger Inverse(BigInteger num, BigInteger mod) {
            BigInteger q, r, t, u1 = BigInteger.One, u2 = BigInteger.Zero, v1 = BigInteger.Zero, v2 = BigInteger.One,
                        a = num, b = mod;
            while (b != BigInteger.Zero) {
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
            if (u1 < BigInteger.Zero) {
                u1 += mod;
            }
            return u1;
        }

        static Tuple<BigInteger, BigInteger> GenerateKeyPair(BigInteger p, BigInteger q) {
            BigInteger d, e;
            e = new BigInteger(Math.Pow(2, 16) + 1);
            BigInteger phi_n = (p - BigInteger.One) * (q - BigInteger.One);
            d = Inverse(e, phi_n);

            return Tuple.Create(e, d);
        }

        static BigInteger Encrypt(BigInteger M, BigInteger e, BigInteger n)
            => BigInteger.ModPow(M, e, n);

        static BigInteger Decrypt(BigInteger C, BigInteger d, BigInteger n)
            => BigInteger.ModPow(C, d, n);

        static BigInteger Sign(BigInteger M, BigInteger d, BigInteger n)
            => BigInteger.ModPow(M, d, n);

        static bool Verify(BigInteger M, BigInteger S, BigInteger e, BigInteger n)
            => BigInteger.Compare(M, BigInteger.ModPow(S, e, n)) == 0;

        static Tuple<BigInteger, BigInteger> SendKey(BigInteger e1, BigInteger n1, BigInteger n, BigInteger d, BigInteger k) {
            BigInteger S = BigInteger.ModPow(k, d, n);
            BigInteger k1 = BigInteger.ModPow(k, e1, n1);
            BigInteger S1 = BigInteger.ModPow(S, e1, n1);
            Console.WriteLine("S = " + DecToHex(S));
            return Tuple.Create(k1, S1);
        }

        static bool RecieveKey(BigInteger d1, BigInteger n1, BigInteger k1, BigInteger S1, BigInteger e, BigInteger n) {
            BigInteger k = BigInteger.ModPow(k1, d1, n1);
            BigInteger S = BigInteger.ModPow(S1, d1, n1);
            Console.WriteLine("k = " + DecToHex(k));
            Console.WriteLine("S = " + DecToHex(S));

            return Verify(k, S, n, e);
        }

        static void Main(string[] args) {
            var A_pq = GeneratePrimePair(32);
            var A_n = A_pq.Item1 * A_pq.Item2;
            var B_pq = GeneratePrimePair(32);
            var B_n = B_pq.Item1 * B_pq.Item2;
            while(A_n > B_n) {
                A_pq = GeneratePrimePair(32);
                A_n = A_pq.Item1 * A_pq.Item2;
                B_pq = GeneratePrimePair(32);
                B_n = B_pq.Item1 * B_pq.Item2;
            }
            var A_keys = GenerateKeyPair(A_pq.Item1, A_pq.Item2);
            var B_keys = GenerateKeyPair(B_pq.Item1, B_pq.Item2);

            Console.WriteLine("p = " + DecToHex(A_pq.Item1));
            Console.WriteLine("q = " + DecToHex(A_pq.Item2));
            Console.WriteLine("n = " + DecToHex(A_n));
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("p1 = " + DecToHex(B_pq.Item1));
            Console.WriteLine("q1 = " + DecToHex(B_pq.Item2));
            Console.WriteLine("n1 = " + DecToHex(B_n));
            Console.WriteLine("==================================================");
            Console.WriteLine("e = " + DecToHex(A_keys.Item1));
            Console.WriteLine("d = " + DecToHex(A_keys.Item2));
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("e1 = " + DecToHex(B_keys.Item1));
            Console.WriteLine("d1 = " + DecToHex(B_keys.Item2));

            

            bool isWorking = true;
            while(isWorking) {
                Console.WriteLine();
                Console.WriteLine("==================================================");
                Console.WriteLine();

                Console.WriteLine("What do you want to do?");
                Console.WriteLine("----------");
                Console.WriteLine("Enc - Encryption");
                Console.WriteLine("Dec - Decryption");
                Console.WriteLine("Sign - Sign");
                Console.WriteLine("Ver - Verification");
                Console.WriteLine("SK - Send key");
                Console.WriteLine("RK - Receive key");
                Console.WriteLine("End - End");
                Console.WriteLine("----------");
                Console.Write("Your choice: ");
                string action = Console.ReadLine();
                if(action == "End") {
                    isWorking = false;
                    break;
                }
                else if(action == "Enc") {
                    Console.WriteLine("----------");
                    Console.Write("Enter M: ");
                    string M_hex = Console.ReadLine();
                    var M = ParseHex(M_hex);
                    var C = Encrypt(M, A_keys.Item1, A_n);
                    Console.WriteLine("C = " + DecToHex(C));
                    Console.WriteLine("----------");
                } 
                else if(action == "Dec") {
                    Console.Write("Enter C: ");
                    string C_hex = Console.ReadLine();
                    var C = ParseHex(C_hex);
                    Console.Write("Enter d: ");
                    string d_hex = Console.ReadLine();
                    var d = ParseHex(d_hex);
                    Console.Write("Enter n: ");
                    string n_hex = Console.ReadLine();
                    var n = ParseHex(n_hex);
                    var M = Decrypt(C, d, n);
                    Console.WriteLine("M = " + DecToHex(M));
                    Console.WriteLine("----------");
                }
                else if(action == "Sign") {
                    Console.Write("Enter M: ");
                    string M_hex = Console.ReadLine();
                    var M = ParseHex(M_hex);
                    var S = Sign(M, A_keys.Item2, A_n);
                    Console.WriteLine("Sign = " + DecToHex(S));
                    Console.WriteLine("----------");
                }
                else if(action == "Ver") {
                    Console.Write("Enter M: ");
                    string M_hex = Console.ReadLine();
                    var M = ParseHex(M_hex);
                    Console.Write("Enter S: ");
                    string S_hex = Console.ReadLine();
                    var S = ParseHex(S_hex);
                    var result = Verify(M, S, A_keys.Item1, A_n);
                    Console.WriteLine("Verification result: " + result);
                    Console.WriteLine("----------");
                }
                else if(action == "SK") {
                    Console.Write("Enter k: ");
                    string k_hex = Console.ReadLine();
                    var k = ParseHex(k_hex);
                    var result = SendKey(B_keys.Item1, B_n, A_n, A_keys.Item2, k);
                    Console.WriteLine("k1 = " + DecToHex(result.Item1));
                    Console.WriteLine("S1 = " + DecToHex(result.Item2));
                    Console.WriteLine("----------");
                }
                else if (action == "RK") {
                    Console.Write("Enter k1: ");
                    string k1_hex = Console.ReadLine();
                    var k1 = ParseHex(k1_hex);
                    Console.Write("Enter S1: ");
                    string S1_hex = Console.ReadLine();
                    var S1 = ParseHex(S1_hex);
                    var result = RecieveKey(B_keys.Item2, B_n, k1, S1, A_keys.Item1, A_n);
                }
            }

            Console.ReadKey();
        }
    }
}

