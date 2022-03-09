using System.Runtime.InteropServices;

namespace DGP.Genshin.Helper.Extension
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Pixel
    {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;
    }
}
