﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Text.RegularExpressions;
using System.IO;


namespace asymCrypto_1
{
    class Program
    {
        static int byteSequenceLength = (int)Math.Pow(2, 18);
        static byte[] GenerateRandomByteSeed(int size)//функция генерации начального заполнения
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

        static int[] GenerateRandomBitsSeed(int size)//функция генерации начального заполнения
        {
            var rnd = new Random();
            int[] seed = new int[size];
            for (int i = 0; i < size; i++)
            {
                seed[i] = rnd.Next(0, 2);
            }
            if (!seed.Contains(1))
            {
                seed[size - 1] = 1;
            }
            return seed;
        }

        //----------------------------------------------- L E H M E R E ------------------------------------------------------------------

        static BigInteger M = new BigInteger(Math.Pow(2, 32));
        static BigInteger A = new BigInteger(Math.Pow(2, 16) + 1);
        static BigInteger C = new BigInteger(119);

        static BigInteger LehmerNext(BigInteger x)
        {
            BigInteger next = (A * x + C) % M;
            if(next.Sign == -1)
            {
                next = next + M;
            }
            return next;
        }
  
        static byte[] LehmerHigh(BigInteger seed)
        {
            var next = seed;
            byte[] bytes = new byte[byteSequenceLength];
            for (int i = 0; i < byteSequenceLength; i++)
            {
                next = LehmerNext(next);
                var nextArr = next.ToByteArray(true, false);
                if (nextArr.Length < 4) bytes[i] = (byte)0;
                else { 
                    bytes[i] = nextArr[nextArr.Length - 1];
                }
               
            }
            return bytes;
        }

        static byte[] LehmerLow(BigInteger seed)
        {
            var next = seed;
            byte[] bytes = new byte[byteSequenceLength];
            for (int i = 0; i < byteSequenceLength; i++)
            {
                next = LehmerNext(next);
                var nextArr = next.ToByteArray(true,false);
                bytes[i]=nextArr[0];
            }
            return bytes;
        }

        //---------------------------------------------------------  L F S R  ------------------------------------------------------------------------

        static byte[] LFSR_Bytes(int[] seed, double len, List<int> indexes) //общая функция ЛРС возвращающая байты
        {                                                                   // indexes - индексы генератора                  
            int[] bits = new int[8];
            int j = 0;
            List<byte> bytes = new List<byte>();
            int[] current_arr = new int[seed.Length];
            Array.Copy(seed, current_arr, seed.Length);
            for (int i = 0; i < len; i++)
            {
                bits[j] = current_arr[0];
                int s = 0;
                foreach (var index in indexes)
                {
                    s += current_arr[index];
                }
                j++;
                if (j == 8)
                {
                    j = 0;
                    bytes.Add(Convert.ToByte(String.Join("", bits), 2));
                }
                Array.Copy(current_arr, 1, current_arr, 0, current_arr.Length - 1);
                current_arr[current_arr.Length - 1] = s % 2;
            }
            return bytes.ToArray();
        }


        static byte[] L_20(int[] seed, double len)//------------------------------------ L 20
        {
            List<int> indexes = new List<int> { 0, 11, 15, 17 };
            return LFSR_Bytes(seed, len, indexes);
        }

        static byte[] L_89(int[] seed, double len)//------------------------------------- L 89
        {
            List<int> indexes = new List<int> { 0, 51 };
            return LFSR_Bytes(seed, len, indexes);
        }
        /*++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/

        static int[] LFSR_Bits(int[] seed, double len, List<int> indexes) //общая функция ЛРС возвращающая биты
        {
            int[] bits = new int[(int)len];
            int[] current_arr = new int[seed.Length];
            Array.Copy(seed, current_arr, seed.Length);
            for (int i = 0; i < len; i++)
            {
                bits[i] = current_arr[0];
                int s = 0;
                foreach (var index in indexes)
                {
                    s += current_arr[index];
                }
                Array.Copy(current_arr, 1, current_arr, 0, current_arr.Length - 1);
                current_arr[current_arr.Length - 1] = s % 2;
            }
            return bits;
        }

        static int[] L_9(int[] seed, double len)
        {
            List<int> indexes = new List<int> { 0, 1, 3, 4 };
            return LFSR_Bits(seed, len, indexes);
        }

        static int[] L_10(int[] seed, double len)
        {
            List<int> indexes = new List<int> { 0, 3 };
            return LFSR_Bits(seed, len, indexes);
        }

        static int[] L_11(int[] seed, double len)
        {
            List<int> indexes = new List<int> { 0, 2 };
            return LFSR_Bits(seed, len, indexes);
        }

        static byte[] Geffe(int[] l9_seq, int[] l10_seq, int[] l11_seq)//------------------------------------------------- G E F F E
        {
            int[] bits = new int[8];
            int j = 0;
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < byteSequenceLength * 8; i++)
            {
                bits[j] = (l10_seq[i] & l11_seq[i]) ^ ((1 ^ l10_seq[i]) & l9_seq[i]);
                j++;
                if (j == 8)
                {
                    j = 0;
                    bytes.Add(Convert.ToByte(String.Join("", bits), 2));
                }
            }
            return bytes.ToArray();
        }

        //-------------------------------------------------------  W O L F R A M  -------------------------------------------------------------------

        static int[] ShiftToHigh(int[] arr)
        {
            int value = arr[0];
            int[] output = new int[arr.Length];
            Array.Copy(arr, 1, output, 0, arr.Length - 2);
            output[arr.Length - 1] = value;
            return output;
        }

        static int[] ShiftToLow(int[] arr)
        {
            int value = arr[arr.Length - 1];
            int[] output = new int[arr.Length];
            Array.Copy(arr, 0, output, 1, arr.Length - 1);
            output[0] = value;
            return output;
        }

        static byte[] Wolfram(int[] r0)
        {
            List<byte> bytes = new List<byte>();
            int[] bits = new int[8];
            int j = 0;
            int[] current_arr = new int[r0.Length];
            Array.Copy(r0, current_arr, r0.Length);
            for (int i = 0; i < byteSequenceLength * 8; i++)
            {
                bits[j] = current_arr[31];

                int[] temp1 = ShiftToLow(current_arr);
                int[] temp2 = ShiftToHigh(current_arr);
               
                for (int k = 0; k < 32; k++)
                {
                    current_arr[k] = temp1[k] ^ (current_arr[k] | temp2[k]);
                }
                j++;
                if (j == 8)
                {
                    j = 0;
                    bytes.Add(Convert.ToByte(String.Join("", bits), 2));
                }
            }
            return bytes.ToArray();
        }

        //--------------------------------------------------------  L I B R A R I A N  --------------------------------------------------------------
        static string Filter(string input)
        {
            string temp = Regex.Replace(input, @"\W", "").ToLower();
            string output = Regex.Replace(temp, @"\d", "");
            return output;
        }
        static byte[] Librarian(string pathInput)
        {
            byte[] bytes = new byte[byteSequenceLength];
            string content = File.ReadAllText(pathInput);
            string filteredText = Filter(content);
            for (int i = 0; i < byteSequenceLength; i++)
            {
                bytes[i] = (byte)filteredText[i];
            }
            return bytes;
        }


        //---------------------------------------------------------------  B M  ---------------------------------------------------------------------

        //static BigInteger pBM1 = BigInteger.Parse("CEA42B987C44FA642D80AD9F51F10457690DEF10C83D0BC1BCEE12FC3B6093E3", System.Globalization.NumberStyles.AllowHexSpecifier);
        //static BigInteger aBM = BigInteger.Parse("5B88C41246790891C095E2878880342E88C79974303BD0400B090FE38A688356", System.Globalization.NumberStyles.AllowHexSpecifier);

        static BigInteger pBM = BigInteger.Parse("93466510612868436543809057926265637055082661966786875228460721852868821292003"),
                          aBM = BigInteger.Parse("41402113656871763270073938229168765778879686959780184368336806110280536326998"),
                          constBMBits = (pBM - BigInteger.One) / (new BigInteger(2)),
                          constBMBytes = (pBM - BigInteger.One) / (new BigInteger(256));

        static byte[] GenerateBMSeed()
        {
            Random rnd = new Random();
            byte[] max = (pBM - BigInteger.One).ToByteArray(true, false);
            var maxByteValue = max[max.Length - 1];
            byte[] seed = new byte[32];
            rnd.NextBytes(seed);
            seed[31] = (byte)rnd.Next(0, (int)maxByteValue);
            return seed;
        }


        static byte[] BMBits(BigInteger t0)
        {
            int[] bits = new int[8];                 
            int j = 0;                                 
            List<byte> bytes = new List<byte>();             
            var tNext = t0;
            for(int i = 0; i < byteSequenceLength*8; i++)
            {
                if(tNext < constBMBits) {
                    bits[j] = 1;
                }
                else
                {
                    bits[j] = 0;
                }
                j++;
                if (j == 8)
                {
                    j = 0;
                    bytes.Add(Convert.ToByte(String.Join("", bits), 2));
                }
                tNext = BigInteger.ModPow(aBM, tNext >= BigInteger.Zero ? tNext : tNext + pBM, pBM);
            }
            return bytes.ToArray();
        }

        static byte[] BMBytes(BigInteger t0)
        {
            byte[] bytes = new byte[byteSequenceLength];
            var tNext = t0;
            bool condition1, condition2;
            for (int i = 0; i < byteSequenceLength; i++)
            {
                for(int k = 0; k < 256; k++)
                {
                    condition1 = tNext > (constBMBytes * (new BigInteger(k)));       
                    condition2 = tNext <= (constBMBytes * (new BigInteger(k) + BigInteger.One));
                    if(condition1 && condition2)
                    {
                        bytes[i] = (byte)k;
                        break;
                    }
                }
                tNext = BigInteger.ModPow(aBM, tNext >= BigInteger.Zero ? tNext : tNext + pBM, pBM);
            }
            return bytes;
        }


        //---------------------------------------------------------------  B B S  ---------------------------------------------------------------------

        static BigInteger pBBS = BigInteger.Parse("D5BBB96D30086EC484EBA3D7F9CAEB07", System.Globalization.NumberStyles.AllowHexSpecifier),
                          qBBS = BigInteger.Parse("425D2B9BFDB25B9CF6C416CC6E37B59C1F", System.Globalization.NumberStyles.AllowHexSpecifier),
                          nBBS = pBBS*qBBS;
        static byte[] BBSBytes(BigInteger r0)//рандомный биг инт за 4 байта
        {
            if(r0 < new BigInteger(2))
            {
                r0 = new BigInteger(2);
            }
            var rNext = r0;
            byte[] bytes = new byte[byteSequenceLength];
            for (int i = 0; i < byteSequenceLength; i++)
            {
                rNext = BigInteger.ModPow(rNext, 2, nBBS);
                var rNextArray = rNext.ToByteArray(true, false);
                bytes[i] = rNextArray[0];
            }
            return bytes;
        }

        static byte[] BBSBits(BigInteger r0)
        {
            if (r0 < new BigInteger(2))
            {
                r0 = new BigInteger(2);
            }
            var rNext = r0;
            int[] bits = new int[8];
            int j = 0;
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < byteSequenceLength*8; i++)
            {
                rNext = BigInteger.ModPow(rNext, 2, nBBS);
                var rNextArray = rNext.ToByteArray(true, false);
                bits[j] = rNextArray[0] % 2;
                j++;
                if (j == 8)
                {
                    j = 0;
                    bytes.Add(Convert.ToByte(String.Join("", bits), 2));
                }

            }
            return bytes.ToArray();
        }

        //---------------------------------------------------------------  T E S T S ------------------------------------------------------------------


        //--------------------------- E Q U I P R O B A B I L I T Y
        static int[] CountBytes(byte[] input) 
        {
            int[] counts = new int[256];
            for (int i = 0; i < input.Length; i++)
            {
                counts[input[i]]++;
            }

            return counts;
        }

        static Tuple<double, bool> EquiprobabilityTEST(int[] bytesCount, double theorHiSquared)
        {
            double hiSquared = bytesCount.Sum(el => Math.Pow(el - (byteSequenceLength / 256), 2) / (byteSequenceLength / 256));
            bool result = hiSquared <= theorHiSquared;
            return Tuple.Create(hiSquared, result);
        }



            //----------------------- I N D E P E N D E N C E  
            static Tuple<int[,],int[],int[]> CountItemsIndependence(byte[] bytes)
        {                                           
            int[,] pairsFreq = new int[256, 256];
            int[] vi = new int[256];
            int[] aj = new int[256];
            for (int i = 0; i < bytes.Length-1; i+=2)
            {
                pairsFreq[bytes[i], bytes[i+1]]++;
                vi[bytes[i]]++;
                aj[bytes[i+1]]++;
            }
            return Tuple.Create(pairsFreq, vi, aj);// 1 - frequency of a pair, 2 - frequency of 1 el, frequency of 2 el
        }

        static Tuple<double,bool> IndependenceTEST(Tuple<int[,], int[], int[]> counts, double theorHiSquared)
        {
            double n = byteSequenceLength / 2;
            double s = 0, hiSquared;
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    if (counts.Item2[i] != 0 && counts.Item3[j] != 0)
                    {
                        s += Math.Pow(counts.Item1[i, j], 2) / (counts.Item2[i] * counts.Item3[j]);
                    }
                }
            }
            hiSquared = n * (s - 1.0);
            bool result = hiSquared <= theorHiSquared;
            return Tuple.Create(hiSquared,result);
        }

        //----------------------- U N I F O R M I T Y
        static double CalculateTheorHiSquared(double quantile, double l)// l зависит от теста:  1 - l =255
             => Math.Round(Math.Sqrt(2 * l) * quantile + l,8);                            //    2 - l = 255*255
                                                                                          //    3 - l = 255*(r-1)

        static Tuple<double[,], double[]> CountItemsUniformity(byte[] bytes, int r)
        {
            int m = byteSequenceLength / r,
                n = m * r;

            double[,] vij = new double[256, r];
            double[] vi = new double[256];

            int rCount = -1;

            for (int i = 0; i < n; i++)
            {
                if (i % m == 0)
                {
                    rCount++;
                }
                vij[bytes[i], rCount]++;
            }
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < r; j++)
                {
                    vi[i] += vij[i, j];
                }
            }

            return Tuple.Create(vij, vi);
        }
        static Tuple<double, bool> UniformityTEST(Tuple<double[,],double[]> counts, double theorHiSquared)
        {
            int m = byteSequenceLength / counts.Item1.GetLength(1),
                n = m * counts.Item1.GetLength(1);
            double hiSquared, s=0;
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < counts.Item1.GetLength(1); j++)
                {
                    if (counts.Item2[i] != 0)
                    {
                        s += Math.Pow(counts.Item1[i, j], 2) / (counts.Item2[i] * m);
                    }
                }
            }

            hiSquared = n * (s - 1.0);
            bool result = hiSquared <= theorHiSquared;
            return Tuple.Create(hiSquared, result);
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------

      static void DisplayTestResut(string pathOutput,string generatorName, string testName,double alpha, double hiSquared, double theorHiSquared, bool result)
        {
            File.AppendAllText(pathOutput, generatorName.PadRight(15) + " | " + testName.PadRight(20) + " | " + alpha.ToString().PadRight(7) + " | " + hiSquared.ToString().PadRight(20) + " | " + theorHiSquared.ToString().PadRight(15) + " | " + result +'\n');
            File.AppendAllText(pathOutput, "--------------------------------------------------------------------------------------------------" + "\n");
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        static void Main(string[] args)
        {
            //-------------------------------- ДЛЯ ТЕСТОВ
            double[] Alpha = new double[] { 0.01, 0.05, 0.1 };
            double[] Quantiles = new double[] { 2.326, 1.645, 1.282 }; // для альфа = 0.01, 0.05, 0.1

            double[,] TheoryChiSquared = new double[,]
            {{307.5284, 292.1493, 283.9516},               // 0.01 test1;  0.05 test1;  0.1 test1; 
            {65863.8124, 65618.2272, 65487.3205},          // 0.01 test2;  0.05 test2;  0.1 test2;
            {0, 0, 0}};                                    // 0.01 test3;  0.05 test3;  0.1 test3;

            string pathInput = @"C:/Users/luciu/source/repos/kant.txt";
            string outputPath = @"C:/Users/luciu/Desktop/asymCryptoOutput/output.txt";
            string outputGeffe11 = @"C:/Users/luciu/Desktop/asymCryptoOutput/output_geffe11.txt";


            File.WriteAllText(outputPath, "Generator:".PadRight(15) + " | " + "Test:".PadRight(20) + " | " + "Alpha :".PadRight(7) + " | " + "X^2 :".PadRight(20) + " | " + "X^2_alpha :".PadRight(15) + " | Result: " + "\n");
            File.AppendAllText(outputPath, "\n"+"--------------------------------------------------------------------------------------------------" + "\n");



            var seed = GenerateRandomByteSeed(4);
            int r = 15;

            var inbuildBytes = GenerateRandomByteSeed(byteSequenceLength);
            for (int i = 0; i < 3; i++)
            {
                var inbuildBytesEquiprobResults = EquiprobabilityTEST(CountBytes(inbuildBytes), TheoryChiSquared[0, i]);
                DisplayTestResut(outputPath, "Inbuild", "Equiprobability", Alpha[i], inbuildBytesEquiprobResults.Item1, TheoryChiSquared[0, i], inbuildBytesEquiprobResults.Item2);
                var inbuildBytesIndependResults = IndependenceTEST(CountItemsIndependence(inbuildBytes), TheoryChiSquared[1, i]);
                DisplayTestResut(outputPath, "Inbuild", "Independence", Alpha[i], inbuildBytesIndependResults.Item1, TheoryChiSquared[1, i], inbuildBytesIndependResults.Item2);
                TheoryChiSquared[2, i] = CalculateTheorHiSquared(Quantiles[i], 255 * (r - 1));
                var inbuildBytesUniformResults = UniformityTEST(CountItemsUniformity(inbuildBytes, r), TheoryChiSquared[2, i]);
                DisplayTestResut(outputPath, "Inbuild", "Uniformity", Alpha[i], inbuildBytesUniformResults.Item1, TheoryChiSquared[2, i], inbuildBytesUniformResults.Item2);
            }
            File.AppendAllText(outputPath, "\n");

            var LehmerLowBytes = LehmerLow(new BigInteger(seed));
            for (int i = 0; i < 3; i++)
            {
                var LehmerLowBytesEquiprobResults = EquiprobabilityTEST(CountBytes(LehmerLowBytes), TheoryChiSquared[0, i]);
                DisplayTestResut(outputPath, "LehmerLow", "Equiprobability", Alpha[i], LehmerLowBytesEquiprobResults.Item1, TheoryChiSquared[0, i], LehmerLowBytesEquiprobResults.Item2);
                var LehmerLowBytesIndependResults = IndependenceTEST(CountItemsIndependence(LehmerLowBytes), TheoryChiSquared[1, i]);
                DisplayTestResut(outputPath, "LehmerLow", "Independence", Alpha[i], LehmerLowBytesIndependResults.Item1, TheoryChiSquared[1, i], LehmerLowBytesIndependResults.Item2);
                TheoryChiSquared[2, i] = CalculateTheorHiSquared(Quantiles[i], 255 * (r - 1));
                var LehmerLowBytesUniformResults = UniformityTEST(CountItemsUniformity(LehmerLowBytes, r), TheoryChiSquared[2, i]);
                DisplayTestResut(outputPath, "LehmerLow", "Uniformity", Alpha[i], LehmerLowBytesUniformResults.Item1, TheoryChiSquared[2, i], LehmerLowBytesUniformResults.Item2);
            }
            File.AppendAllText(outputPath, "\n");

            var LehmerHighBytes = LehmerHigh(new BigInteger(seed));
            for (int i = 0; i < 3; i++)
            {
                var LehmerHighBytesEquiprobResults = EquiprobabilityTEST(CountBytes(LehmerHighBytes), TheoryChiSquared[0, i]);
                DisplayTestResut(outputPath, "LehmerHigh", "Equiprobability", Alpha[i], LehmerHighBytesEquiprobResults.Item1, TheoryChiSquared[0, i], LehmerHighBytesEquiprobResults.Item2);
                var LehmerHighBytesIndependResults = IndependenceTEST(CountItemsIndependence(LehmerHighBytes), TheoryChiSquared[1, i]);
                DisplayTestResut(outputPath, "LehmerHigh", "Independence", Alpha[i], LehmerHighBytesIndependResults.Item1, TheoryChiSquared[1, i], LehmerHighBytesIndependResults.Item2);
                TheoryChiSquared[2, i] = CalculateTheorHiSquared(Quantiles[i], 255 * (r - 1));
                var LehmerHighBytesUniformResults = UniformityTEST(CountItemsUniformity(LehmerHighBytes, r), TheoryChiSquared[2, i]);
                DisplayTestResut(outputPath, "LehmerHigh", "Uniformity", Alpha[i], LehmerHighBytesUniformResults.Item1, TheoryChiSquared[2, i], LehmerHighBytesUniformResults.Item2);
            }
            File.AppendAllText(outputPath, "\n");

            var l20Bytes = L_20(GenerateRandomBitsSeed(20), byteSequenceLength * 8);
            for (int i = 0; i < 3; i++)
            {
                var l20BytesEquiprobResults = EquiprobabilityTEST(CountBytes(l20Bytes), TheoryChiSquared[0, i]);
                DisplayTestResut(outputPath, "L_20", "Equiprobability", Alpha[i], l20BytesEquiprobResults.Item1, TheoryChiSquared[0, i], l20BytesEquiprobResults.Item2);
                var l20BytesIndependResults = IndependenceTEST(CountItemsIndependence(l20Bytes), TheoryChiSquared[1, i]);
                DisplayTestResut(outputPath, "L_20", "Independence", Alpha[i], l20BytesIndependResults.Item1, TheoryChiSquared[1, i], l20BytesIndependResults.Item2);
                TheoryChiSquared[2, i] = CalculateTheorHiSquared(Quantiles[i], 255 * (r - 1));
                var l20BytesUniformResults = UniformityTEST(CountItemsUniformity(l20Bytes, r), TheoryChiSquared[2, i]);
                DisplayTestResut(outputPath, "L_20", "Uniformity", Alpha[i], l20BytesUniformResults.Item1, TheoryChiSquared[2, i], l20BytesUniformResults.Item2);
            }
            File.AppendAllText(outputPath, "\n");

            var l89Bytes = L_89(GenerateRandomBitsSeed(89), byteSequenceLength * 8);
            for (int i = 0; i < 3; i++)
            {
                var l89BytesEquiprobResults = EquiprobabilityTEST(CountBytes(l89Bytes), TheoryChiSquared[0, i]);
                DisplayTestResut(outputPath, "L_89", "Equiprobability", Alpha[i], l89BytesEquiprobResults.Item1, TheoryChiSquared[0, i], l89BytesEquiprobResults.Item2);
                var l89BytesIndependResults = IndependenceTEST(CountItemsIndependence(l89Bytes), TheoryChiSquared[1, i]);
                DisplayTestResut(outputPath, "L_89", "Independence", Alpha[i], l89BytesIndependResults.Item1, TheoryChiSquared[1, i], l89BytesIndependResults.Item2);
                TheoryChiSquared[2, i] = CalculateTheorHiSquared(Quantiles[i], 255 * (r - 1));
                var l89BytesUniformResults = UniformityTEST(CountItemsUniformity(l20Bytes, r), TheoryChiSquared[2, i]);
                DisplayTestResut(outputPath, "L_89", "Uniformity", Alpha[i], l89BytesUniformResults.Item1, TheoryChiSquared[2, i], l89BytesUniformResults.Item2);
            }
            File.AppendAllText(outputPath, "\n");

            var l9 = L_9(GenerateRandomBitsSeed(9), byteSequenceLength * 8);
                var l10 = L_10(GenerateRandomBitsSeed(10), byteSequenceLength * 8);
                var l11 = L_9(GenerateRandomBitsSeed(11), byteSequenceLength * 8);
                var geffeBytes = Geffe(l9, l10, l11);

             for (int i = 0; i < 3; i++)
              {
             var geffeBytesEquiprobResults = EquiprobabilityTEST(CountBytes(geffeBytes), TheoryChiSquared[0, i]);
             DisplayTestResut(outputPath, "Geffe", "Equiprobability", Alpha[i], geffeBytesEquiprobResults.Item1, TheoryChiSquared[0, i], geffeBytesEquiprobResults.Item2);
             var geffeBytesIndependResults = IndependenceTEST(CountItemsIndependence(geffeBytes), TheoryChiSquared[1, i]);
             DisplayTestResut(outputPath, "Geffe", "Independence", Alpha[i], geffeBytesIndependResults.Item1, TheoryChiSquared[1, i], geffeBytesIndependResults.Item2);
             TheoryChiSquared[2, i] = CalculateTheorHiSquared(Quantiles[i], 255 * (r - 1));
             var geffeBytesUniformResults = UniformityTEST(CountItemsUniformity(geffeBytes, r), TheoryChiSquared[2, i]);
             DisplayTestResut(outputPath, "Geffe", "Uniformity", Alpha[i], geffeBytesUniformResults.Item1, TheoryChiSquared[2, i], geffeBytesUniformResults.Item2);
            }
            File.AppendAllText(outputPath, "\n");

            var bitsSeed = GenerateRandomBitsSeed(32);

            var wolframBytes = Wolfram(bitsSeed);
            for (int i = 0; i < 3; i++)
            {
                var wolframBytesEquiprobResults = EquiprobabilityTEST(CountBytes(wolframBytes), TheoryChiSquared[0, i]);
                DisplayTestResut(outputPath, "Wolfram", "Equiprobability", Alpha[i], wolframBytesEquiprobResults.Item1, TheoryChiSquared[0, i], wolframBytesEquiprobResults.Item2);
                var wolframBytesIndependResults = IndependenceTEST(CountItemsIndependence(wolframBytes), TheoryChiSquared[1, i]);
                DisplayTestResut(outputPath, "Wolfram", "Independence", Alpha[i], wolframBytesIndependResults.Item1, TheoryChiSquared[1, i], wolframBytesIndependResults.Item2);
                TheoryChiSquared[2, i] = CalculateTheorHiSquared(Quantiles[i], 255 * (r - 1));
                var wolframBytesUniformResults = UniformityTEST(CountItemsUniformity(geffeBytes, r), TheoryChiSquared[2, i]);
                DisplayTestResut(outputPath, "Wolfram", "Uniformity", Alpha[i], wolframBytesUniformResults.Item1, TheoryChiSquared[2, i], wolframBytesUniformResults.Item2);
            }
            File.AppendAllText(outputPath, "\n");

            var LibrarianBytes = Librarian(pathInput);
            for (int i = 0; i < 3; i++)
            {
                var LibrarianBytesEquiprobResults = EquiprobabilityTEST(CountBytes(LibrarianBytes), TheoryChiSquared[0, i]);
                DisplayTestResut(outputPath, "Librarian", "Equiprobability", Alpha[i], LibrarianBytesEquiprobResults.Item1, TheoryChiSquared[0, i], LibrarianBytesEquiprobResults.Item2);
                var LibrarianBytesIndependResults = IndependenceTEST(CountItemsIndependence(LibrarianBytes), TheoryChiSquared[1, i]);
                DisplayTestResut(outputPath, "Librarian", "Independence", Alpha[i], LibrarianBytesIndependResults.Item1, TheoryChiSquared[1, i], LibrarianBytesIndependResults.Item2);
                TheoryChiSquared[2, i] = CalculateTheorHiSquared(Quantiles[i], 255 * (r - 1));
                var LibrarianBytesUniformResults = UniformityTEST(CountItemsUniformity(LibrarianBytes, r), TheoryChiSquared[2, i]);
                DisplayTestResut(outputPath, "Librarian", "Uniformity", Alpha[i], LibrarianBytesUniformResults.Item1, TheoryChiSquared[2, i], LibrarianBytesUniformResults.Item2);
            }
            File.AppendAllText(outputPath, "\n");

            var bbsBytes = BBSBytes(new BigInteger(seed));
            for (int i = 0; i < 3; i++)
            {
                var bbsBytesEquiprobResults = EquiprobabilityTEST(CountBytes(bbsBytes), TheoryChiSquared[0, i]);
                DisplayTestResut(outputPath, "BBS bytes", "Equiprobability", Alpha[i], bbsBytesEquiprobResults.Item1, TheoryChiSquared[0, i], bbsBytesEquiprobResults.Item2);
                var bbsBytesIndependResults = IndependenceTEST(CountItemsIndependence(bbsBytes), TheoryChiSquared[1, i]);
                DisplayTestResut(outputPath, "BBS bytes", "Independence", Alpha[i], bbsBytesIndependResults.Item1, TheoryChiSquared[1, i], bbsBytesIndependResults.Item2);
                TheoryChiSquared[2, i] = CalculateTheorHiSquared(Quantiles[i], 255 * (r - 1));
                var k = CountItemsUniformity(bbsBytes, r);
                var bbsBytesUniformResults = UniformityTEST(CountItemsUniformity(bbsBytes, r), TheoryChiSquared[2, i]);
                DisplayTestResut(outputPath, "BBS bytes", "Uniformity", Alpha[i], bbsBytesUniformResults.Item1, TheoryChiSquared[2, i], bbsBytesUniformResults.Item2);
            }
            File.AppendAllText(outputPath, "\n");

            var bbsBits = BBSBits(new BigInteger(seed));
            for (int i = 0; i < 3; i++)
            {
                var bbsBitsEquiprobResults = EquiprobabilityTEST(CountBytes(bbsBits), TheoryChiSquared[0, i]);
                DisplayTestResut(outputPath, "BBS bits", "Equiprobability", Alpha[i], bbsBitsEquiprobResults.Item1, TheoryChiSquared[0, i], bbsBitsEquiprobResults.Item2);
                var bbsBitsIndependResults = IndependenceTEST(CountItemsIndependence(bbsBits), TheoryChiSquared[1, i]);
                DisplayTestResut(outputPath, "BBS bits", "Independence", Alpha[i], bbsBitsIndependResults.Item1, TheoryChiSquared[1, i], bbsBitsIndependResults.Item2);
                TheoryChiSquared[2, i] = CalculateTheorHiSquared(Quantiles[i], 255 * (r - 1));
                var bbsBitsUniformResults = UniformityTEST(CountItemsUniformity(bbsBits, r), TheoryChiSquared[2, i]);
                DisplayTestResut(outputPath, "BBS bits", "Uniformity", Alpha[i], bbsBitsUniformResults.Item1, TheoryChiSquared[2, i], bbsBitsUniformResults.Item2);
            }
            File.AppendAllText(outputPath, "\n");

            var bmBytes = BMBytes(new BigInteger(GenerateBMSeed()));
            for (int i = 0; i < 3; i++)
            {
                var bmBytesEquiprobResults = EquiprobabilityTEST(CountBytes(bmBytes), TheoryChiSquared[0, i]);
                DisplayTestResut(outputPath, "BM bytes", "Equiprobability", Alpha[i], bmBytesEquiprobResults.Item1, TheoryChiSquared[0, i], bmBytesEquiprobResults.Item2);
                var bmBytesIndependResults = IndependenceTEST(CountItemsIndependence(bmBytes), TheoryChiSquared[1, i]);
                DisplayTestResut(outputPath, "BM bytes", "Independence", Alpha[i], bmBytesIndependResults.Item1, TheoryChiSquared[1, i], bmBytesIndependResults.Item2);
                TheoryChiSquared[2, i] = CalculateTheorHiSquared(Quantiles[i], 255 * (r - 1));
                var bmBytesUniformResults = UniformityTEST(CountItemsUniformity(bmBytes, r), TheoryChiSquared[2, i]);
                DisplayTestResut(outputPath, "BM bytes", "Uniformity", Alpha[i], bmBytesUniformResults.Item1, TheoryChiSquared[2, i], bmBytesUniformResults.Item2);
            }
            File.AppendAllText(outputPath, "\n");

            var bmBits = BMBits(new BigInteger(GenerateBMSeed()));
            for (int i = 0; i < 3; i++)
            {
                var bmBitsEquiprobResults = EquiprobabilityTEST(CountBytes(bmBits), TheoryChiSquared[0, i]);
                DisplayTestResut(outputPath, "BM bits", "Equiprobability", Alpha[i], bmBitsEquiprobResults.Item1, TheoryChiSquared[0, i], bmBitsEquiprobResults.Item2);
                var bmBitsIndependResults = IndependenceTEST(CountItemsIndependence(bmBits), TheoryChiSquared[1, i]);
                DisplayTestResut(outputPath, "BM bits", "Independence", Alpha[i], bmBitsIndependResults.Item1, TheoryChiSquared[1, i], bmBitsIndependResults.Item2);
                TheoryChiSquared[2, i] = CalculateTheorHiSquared(Quantiles[i], 255 * (r - 1));
                var bmBitsUniformResults = UniformityTEST(CountItemsUniformity(bmBits, r), TheoryChiSquared[2, i]);
                DisplayTestResut(outputPath, "BM bits", "Uniformity", Alpha[i], bmBitsUniformResults.Item1, TheoryChiSquared[2, i], bmBitsUniformResults.Item2);
            }
            File.AppendAllText(outputPath, "\n");

            Console.ReadKey();
        }
    }
}
