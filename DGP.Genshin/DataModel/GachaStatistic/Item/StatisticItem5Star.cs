using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;

namespace DGP.Genshin.DataModel.GachaStatistic.Item
{
    /// <summary>
    /// 带有个数统计的奖池统计5星物品
    /// </summary>
    public class StatisticItem5Star
    {
        public string? Source { get; set; }
        public string? Name { get; set; }
        /// <summary>
        /// 计数器或用于记录垫抽数
        /// </summary>
        public int Count { get; set; }
        public DateTime Time { get; set; }
        public bool IsBigGuarantee { get; set; }
        public bool IsUp { get; set; }
        public string? GachaTypeName { get; set; }

        public SolidColorBrush Background
        {
            get
            {
                byte[] codes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(this.Name!));
                Color color = Color.FromRgb(
                    (byte)(codes[0] / 2 + 64),
                    (byte)(codes[1] / 2 + 64),
                    (byte)(codes[2] / 2 + 64));
                return new SolidColorBrush(color);
            }
        }
        public SolidColorBrush TranslucentBackground
        {
            get
            {
                byte[] codes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(this.Name!));
                Color color = Color.FromArgb(
                    102,
                    (byte)(codes[0] / 2 + 64),
                    (byte)(codes[1] / 2 + 64),
                    (byte)(codes[2] / 2 + 64));
                return new SolidColorBrush(color);
            }
        }
    }
}
