using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DGP.Snap.Test
{
    internal class Program
    {
        //crack down the salt
        private static readonly string template = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static void Main(string[] args)
        {
            Console.WriteLine("ready!");
            List<string> salts = new List<string>();
            CheckSalt(1628558499L, "ijcm88", "78a0a082de6f12720b2a196e310a19d3", salts, "");
            foreach (string s in salts)
            {
                Console.WriteLine($"Possible SALT:{s}");
            }
            Console.ReadKey();
        }

        private static void CheckSalt(long time, string random, string result, List<string> salts, string saltbase)
        {
            for (int j = 0; j < template.Length; j++)
            {
                if (saltbase.Length < 32)
                {
                    string saltAfter = saltbase + template[j];
                    string computed = CreateDS(time, random, saltAfter);
                    Console.WriteLine($"{time}|{random}|{computed}|{salts.Count}|{saltAfter}|{computed == result}");
                    if (computed == result)
                    {
                        salts.Add(saltAfter);
                        Console.WriteLine("按键继续");
                        Console.ReadKey();
                    }

                    CheckSalt(time, random, result, salts, saltAfter);
                }
            }
        }

        #region Encryption
        private static string CreateDS(long time, string random, string APISalt) => ComputeMd5($"salt={APISalt}&t={time}&r={random}");
        private static string ComputeMd5(string content)
        {
            using MD5 md5 = MD5.Create();
            byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(content));

            StringBuilder builder = new StringBuilder();
            foreach (byte b in result)
                builder.Append(b.ToString("x2"));

            return builder.ToString();
        }
        #endregion
    }
}
