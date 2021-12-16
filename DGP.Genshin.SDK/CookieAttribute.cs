namespace DGP.Genshin.SDK
{
    /// <summary>
    /// 指定此字段为Cookie字段，会随主程序的当前Cookie变化而变化
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class CookieAttribute : Attribute
    {

    }
}