using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;

namespace DGP.Genshin.Services.GachaStatistics.Statistics
{
    /// <summary>
    /// 带有个数统计的奖池统计5星物品
    /// </summary>
    public class StatisticItem5Star
    {
        public string? Name { get; set; }
        public int Count { get; set; }
        public DateTime Time { get; set; }
        public bool IsBigGuarantee { get; set; }
        public bool IsUp { get; set; }
        public SolidColorBrush Background
        {
            get
            {
                Debug.Assert(Name is not null);
                byte[] codes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Name));
                Color color = Color.FromRgb((byte)(codes[0] / 2 + 64), (byte)(codes[1] / 2 + 64), (byte)(codes[2] / 2 + 64));
                return new SolidColorBrush(color);
            }
        }
    }
}
