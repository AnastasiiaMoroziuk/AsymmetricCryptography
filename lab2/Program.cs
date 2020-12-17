using System;
using System.Linq;
using System.Numerics;

namespace asymCrypto_2
{
    class Program
    {
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
            ////var nA = BigInteger.Parse("", System.Globalization.NumberStyles.AllowHexSpecifier);
            ////var dA = BigInteger.Parse("", System.Globalization.NumberStyles.AllowHexSpecifier);
            ////var eA = BigInteger.Parse("", System.Globalization.NumberStyles.AllowHexSpecifier);
            ////var phi_nA = BigInteger.Parse("", System.Globalization.NumberStyles.AllowHexSpecifier);

            ////var pA = BigInteger.Parse("7E2C7F4B871D2C2EAF587660F37CA6EB", System.Globalization.NumberStyles.AllowHexSpecifier);
            ////var qA = BigInteger.Parse("7042D42A2EEDC650970099F6B144403A8F", System.Globalization.NumberStyles.AllowHexSpecifier);

            ////var pB = BigInteger.Parse("495FBF6F01EAC752E9A3FF71FE51D747", System.Globalization.NumberStyles.AllowHexSpecifier);
            ////var qB = BigInteger.Parse("1651AF4F6C011403258598985C57283599", System.Globalization.NumberStyles.AllowHexSpecifier);

            ////if (pB * qB < pA * qA)
            ////{
            ////    var tempP = pA;
            ////    var tempQ = qA;
            ////    pA = pB;
            ////    qA = qB;
            ////    pB = tempP;
            ////    qB = tempQ;
            ////}

            ////var nA = pA * qA;
            ////var eA = new BigInteger(Math.Pow(2, 16) + 1);
            ////BigInteger phi_nA = (pA - BigInteger.One) * (qA - BigInteger.One);
            ////var dA = Inverse(eA, phi_nA);
            var MA = BigInteger.Parse("66A0960A6704230455AB8013276AD3022C2DC5F0158B29EBCF45B229D8EBDEF0", System.Globalization.NumberStyles.AllowHexSpecifier);




            ////var nB = pB * qB;
            ////var eB = new BigInteger(Math.Pow(2, 16) + 1);
            ////BigInteger phi_nB = (pB - BigInteger.One) * (qB - BigInteger.One);
            ////var dB = Inverse(eB, phi_nB);

            //var MB = BigInteger.Parse("BEA4BA4EC0BD4E8141E3E8DBB3AD5842378D8253E91544C96D994E2DBAB6EFE0", System.Globalization.NumberStyles.AllowHexSpecifier);



            var keysA = GenerateKeyPair();
            var keysB = GenerateKeyPair();

            while (keysB.Item2.Item1 < keysA.Item2.Item1) // while (n1 < n) => generate new p,q,n...
            {
                keysA = GenerateKeyPair();
                keysB = GenerateKeyPair();
            }

            //----------------------------------------------------- Encryption-Decryption for Abonent A   -----------------------------------------------------------------------
            Console.WriteLine("\nABONENT A: \n");
            BigInteger nA = keysA.Item2.Item1,
                       eA = keysA.Item2.Item2,
                       dA = keysA.Item1.Item1;

            //BigInteger MA = new BigInteger(GenerateRandomByteSeed(62));
            BigInteger CA = Encrypt(MA, nA, eA);
            BigInteger MA_decrypted = Decrypt(CA, dA, nA);
            BigInteger SA = Sign(MA, nA, dA);
            bool verA = Verify(MA, SA, nA, eA);

            Console.WriteLine("n = " + nA.ToString("X"));
            Console.WriteLine("e = " + eA.ToString("X"));
            Console.WriteLine("d = " + dA.ToString("X"));
            Console.WriteLine("M = " + MA.ToString("X"));
            Console.WriteLine("C = " + CA.ToString("X"));
            Console.WriteLine("M_decrypted = " + MA_decrypted.ToString("X"));
            Console.WriteLine("S = " + SA.ToString("X"));
            Console.WriteLine(verA ? "Verified" : "Not verified");

            ////----------------------------------------------------- Encryption-Decryption for Abonent B   -----------------------------------------------------------------------
            //Console.WriteLine("\nABONENT B: \n");
            //BigInteger nB = keysB.Item2.Item1,
            //           eB = keysB.Item2.Item2,
            //           dB = keysB.Item1.Item1;

            ////BigInteger MB = new BigInteger(GenerateRandomByteSeed(62));
            //BigInteger CB = Encrypt(MB, nB, eB);
            //BigInteger MB_decrypted = Decrypt(CB, dB, nB);
            //BigInteger SB = Sign(MB, nB, dB);
            //bool verB = Verify(MB, SB, nB, eB);

            //Console.WriteLine("n = " + nB.ToString("X"));
            //Console.WriteLine("e = " + eB.ToString("X"));
            //Console.WriteLine("d = " + dB.ToString("X"));
            //Console.WriteLine("M = " + MB.ToString("X"));
            //Console.WriteLine("C = " + CB.ToString("X"));
            //Console.WriteLine("M_decrypted = " + MB_decrypted.ToString("X"));
            //Console.WriteLine("S = " + SB.ToString("X"));
            //Console.WriteLine(verB ? "Verified" : "Not verified");

            ////---------------------------------------------------- SEND - RECIEVE KEY ----------------------------------------------------------------------------
            ////var k = GenerateBigInteger(nA - BigInteger.One);
            //var k = BigInteger.Parse("DC3591FE994A74FFD7662F90A08251CDA5A13959413AF5DCFBFF6D12CC59DC96", System.Globalization.NumberStyles.AllowHexSpecifier);

            //Console.WriteLine("\n Send: \n");
            //var k1S1 = SendKey(eB, nB, nA, dA, k);

            //Console.WriteLine("k = " + k.ToString("X"));
            //Console.WriteLine("k1 = " + k1S1.Item1.ToString("X"));
            //Console.WriteLine("S1 = " + k1S1.Item2.ToString("X"));

            //Console.WriteLine("\n Recieved: \n");
            //var recieved = RecieveKey(dB, nB, k1S1.Item1, k1S1.Item2, eA, nA);
            //Console.WriteLine(recieved);



            var n = BigInteger.Parse("0A8BAE80602C9C257FF49E6D52E06FA28C09C305194C83771A83C27B31C0555591985FBF6BE73CCD7C01D01AE0446111D98AE0F9360CEE5C283573D3197B6D95", System.Globalization.NumberStyles.AllowHexSpecifier);
            var e = BigInteger.Parse("10001", System.Globalization.NumberStyles.AllowHexSpecifier);
            var d = BigInteger.Parse("79F92361B502450B77F0E47D9718541EC19E270FD28E2A066FA78CCB5DA63D57A61D431BF8F018C0116FD257C200666A97367923F3F8B53E11C3092A7BF3A01", System.Globalization.NumberStyles.AllowHexSpecifier);

            var C = BigInteger.Parse("0100A1586A33B4C9D44F36FA6ABCCE162DBCC05ABF7C4184BC37C923A391F6A91B8ED9884220742E3B66B6108410352DC33339FFEE373C3532CFA81823C9EB81", System.Globalization.NumberStyles.AllowHexSpecifier);
            var M = Decrypt(C, d, n);
            Console.WriteLine(M.ToString("X"));
            Console.ReadKey();
        }
    }
}

