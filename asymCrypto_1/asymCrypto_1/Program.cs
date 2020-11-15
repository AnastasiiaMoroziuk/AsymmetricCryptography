using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;


namespace asymCrypto_1
{
    class Program
    {
        //----------------------------------------------- L E H M E R E ------------------------------------------------------------------

        static BigInteger M = new BigInteger(Math.Pow(2, 32));
        static BigInteger A = new BigInteger(Math.Pow(2, 16) + 1);
        static BigInteger C = new BigInteger(119);

        static BigInteger LehmerNext(BigInteger x)
        {
            return (A * x + C) % M;
        }
  
        static byte[] LehmerHigh(BigInteger seed)
        {
            var next = seed;
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < Math.Pow(2, 18); i++)
            {
                next = LehmerNext(next);
                var nextArr = next.ToByteArray(true, false);
                bytes.Add(nextArr[nextArr.Length - 1]);
            }
            return bytes.ToArray();
        }

        static byte[] LehmerLow(BigInteger seed)
        {
            var next = seed;
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < Math.Pow(2, 18); i++)
            {
                next = LehmerNext(next);
                var nextArr = next.ToByteArray(true, false);
                bytes.Add(nextArr[0]);
            }
            return bytes.ToArray();
        }

        static byte[] Generate_Random_LEHMERE_seed()//функция генерации начального заполнения
        {
            Random rnd = new Random();
            byte[] seed = new byte[4];
            byte[] zeros = new byte[] { 0, 0, 0, 0 };
            rnd.NextBytes(seed);
            if (seed.SequenceEqual(zeros))
            {
                seed[seed.Length - 1] = (byte)1;
            }
            return seed;
        }
        //---------------------------------------------------------  L F S R  ------------------------------------------------------------------------
        static int[] Generate_Random_LFSR_seed(int size)//функция генерации начального заполнения
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

        static byte[] LFSR_Bytes(int[] seed, double len, List<int> indexes) //общая функция ЛРС возвращающая байты
        {                                                        // indexes - индексы генератора                  
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
            for (int i = 0; i < Math.Pow(2, 18) * 8; i++)
            {
                bits[j] = l10_seq[i] & l11_seq[i] ^ (1 ^ l10_seq[i]) & l9_seq[i];
                j++;
                if (j == 8)
                {
                    j = 0;
                    bytes.Add(Convert.ToByte(String.Join("", bits), 2));
                }
            }
            return bytes.ToArray();
        }
        //---------------------------------------------------------------  B B S  ---------------------------------------------------------------------

        static BigInteger p, q, n;
        static byte[] BBSBits()
        {
            byte[] bytes = new byte[(int)Math.Pow(2,18)];
            return bytes;
        }

//---------------------------------------------------------------  T E S T S ------------------------------------------------------------------

        //-----------------E Q U I P R O B A B I L I T Y
        static int[] CountBytes(byte[] input)  
        {
            int[] counts = new int[256];
            for (int i = 0; i < input.Length; i++)
            {
                counts[input[i]]++;
            }
            return counts;
        }

        static bool EquiprobabilityTEST(int[] bytesCount, double theorHiSquared)
            => bytesCount.Sum(el => Math.Pow(el- (Math.Pow(2, 18) / 256),2)/ (Math.Pow(2, 18) / 256)) <= theorHiSquared;


        //----------------------- I N D E P E N D E N C E  
        static Tuple<int[,],int[],int[]> CountItems(byte[] bytes)
        {                                           
            int[,] pairsFreq = new int[256, 256];
            int[] vi = new int[256];
            int[] aj = new int[256];
            for (int i = 0; i < bytes.Length-1; i+=2)
            {
                pairsFreq[bytes[i], bytes[i+1]]++;
                vi[bytes[i]] ++;
                aj[bytes[i+1]]++;
            }
            return Tuple.Create(pairsFreq, vi, aj);// 1 - frequency of a pair, 2 - frequency of 1 el, frequency of 2 el
        }

        static bool IndependenceTEST(Tuple<int[,], int[], int[]> counts, double theorHiSquared)
        {
            double n = Math.Pow(2, 18) / 2; //по хорошему тут не 2^18, а длина послед в общем виде
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
            hiSquared = n * (s - 1);
            return  hiSquared <= theorHiSquared;
        }

        //----------------------- U N I F O R M I T Y
        static bool UniformityTEST(double theoryHiSquared)
            => true;

//----------------------------------------------------------------------------------------------------------------------------------------------



        static void DisplayArray(int[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                Console.Write(a[i]);
            }
            Console.WriteLine("");
        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------
        static void Main(string[] args)
        {
            //var h = Generate_Random_LFSR_seed(20);
            //DisplayArray(h);

            //var m = Generate_Random_LEHMERE_seed();
            //foreach(var n in m) { Console.Write(n+" "); }

            //-------------------------------- РАБОТА L_20
            //int[] l = new int[] { 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1 };
            //Console.WriteLine(l.Length);
            //var l20 = L_20(l, Math.Pow(2, 18) * 8);
            //foreach(var bytic in l20)
            //{
            //    Console.Write(bytic + " ");
            //}

            //-------------------------------- РАБОТА L_89
            //int[] l1 = new int[] { 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1 };
            //Console.WriteLine(l1.Length);
            //var l89 = L_89(l1, Math.Pow(2, 18) * 8);
            //Console.WriteLine(l89.Count);
            //foreach (var bytic in l89)
            //{
            //    Console.Write(bytic + " ");
            //}

            //-------------------------------- ПОДСЧЕТ ВХОЖДЕНИЯ БАЙТОВ
            //var arr = new List<byte> { 255, 67, 0, 0, 9, 88, 5, 5, 3, 255, 0 };
            //var bytes = CountBytes(l89);

            //-------------------------------- ХИ КВАДРАТ TEST 1
            //var hiKvadrat = EquiprobabilityTEST(CountBytes(l89),4);
            //Console.Write(hiKvadrat);

            //-------------------------------- ДЛЯ ТЕСТОВ
            //double[] Alpha = new double[] { 0.01, 0.05, 0.1 };

            //double[,] TheoryChiSquared = new double[,]
            //{{307.5284, 292.1493, 283.9516},         // 0.01 test1;  0.05 test1;  0.1 test1; 
            //{65863.8124, 65618.2272, 65487.3205},          // 0.01 test2;  0.05 test2;  0.1 test2;
            //{0, 0, 0}};         // 0.01 test3;  0.05 test3;  0.1 test3;

            //-------------------------------- РАБОТА Geffe
            //int[] l_11 = new int[] { 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0 };
            //int[] l_10 = new int[] { 1, 0, 0, 1, 1, 0, 0, 0, 1, 0};
            //int[] l_9 = new int[] { 1, 0, 0, 1, 1, 0, 0, 0, 1 }; 

            //var l_11_seq =  L_11(l_11, Math.Pow(2, 18) * 8);
            //var l_10_seq = L_11(l_10, Math.Pow(2, 18) * 8);
            //var l_9_seq = L_11(l_9, Math.Pow(2, 18) * 8);

            //var geffe_seq = Geffe(l_9_seq, l_10_seq, l_11_seq);

            //Console.WriteLine(geffe_seq.Length);
            //foreach (var g in geffe_seq)
            //{
            //    Console.Write(g + " ");
            //}
            //var b = CountBytes(geffe_seq);
            //var hiKvadrat = HiSquared_Equiprobability(b);
            //Console.Write(hiKvadrat);

            //------------------------------------------------------------

            //byte[] b_test = new byte[] { 0,0,1,1,2,1,2,1,1,1,3,1,6,1,7,1,7,1,7,1,5,1};

            //var p = CountItems(b_test);
            //for(int i = 0; i < p.Item1.GetLength(0); i++)
            //{
            //    //for(int j = 0; j < p.Item1.GetLength(1); j++)
            //    //{
            //        //Console.WriteLine("(" + i +","+ j + ") - " + p.Item1[i, j]);
            //        Console.WriteLine(i+" - "+p.Item2[i] + "       "+p.Item3[i]);
            //    //}
            //}
            //var c = CountBytes(b_test);
            //for (int i = 0; i < c.Length; i++)
            //{
            //    Console.WriteLine(i +"-" + c[i]);
            //}
            //Console.WriteLine("test = " + EquiprobabilityTEST(c, 4));
            Console.ReadKey();
        }
    }
}
