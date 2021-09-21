namespace DGP.Snap.Framework.Core
{
    public interface IPartiallyCloneable<T>
    {
        /// <summary>
        /// 返回一个部分克隆的新对象
        /// </summary>
        /// <returns></returns>
        T ClonePartially();
    }
}
