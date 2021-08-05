using DGP.Snap.Framework.Extensions.System;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;

namespace DGP.Snap.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("ready!");
            string input;
            while ((input = Console.ReadLine())!= null){
                GetColor(input);
            }    
        }

        public static SolidColorBrush GetColor(string name)
        {
            MD5 md5 = MD5.Create();
            byte[] codes = md5.ComputeHash(Encoding.UTF8.GetBytes(name));
            Color color = Color.FromRgb(codes[0], codes[1], codes[2]);
            Console.WriteLine($"{name}-{codes:X}-{color}:{color.R},{color.G},{color.B}");
            return new SolidColorBrush(color);
        }
    }
}
