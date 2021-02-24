using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DGP.Snap.Test
{
    internal class Program
    {
        private static void Main(string[] args) => Test();

        public static void Test()
        {
            string s = @"护盾强效提升<color=#99FFFFFF>25%</color>。攻击命中后的8秒内，攻击力提升<color=#99FFFFFF>5%</color>。";
            //match the format required string
            if (s.StartsWith("#"))
                s = s.Remove(0, 1);
            //color
            s = new Regex(@"<color=.*?>").Replace(s, "");
            Console.WriteLine(s);
            s = s.Replace("</color>", "");
            Console.WriteLine(s);
            //important mark
            //s = s.Replace("<i>", "").Replace("</i>", "");
            //nickname
            s = s.Replace("{NICKNAME}", "[!:玩家昵称]");
            //apply \n & \r char
            s = s.Replace(@"\n", "\n").Replace(@"\r", "\r");
            Debug.WriteLine(s);
            Console.ReadKey();
        }
    }
}
