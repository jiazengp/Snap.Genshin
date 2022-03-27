using System;

namespace DGP.Genshin.Message
{
    public class NavigateRequestMessage : TypedMessage<Type>
    {
        public NavigateRequestMessage(Type pageType, bool isSyncTabRequested = false, object? extraData = null) : base(pageType)
        {
            this.IsSyncTabRequested = isSyncTabRequested;
            this.ExtraData = extraData;
        }
        public bool IsSyncTabRequested { get; set; }
        public object? ExtraData { get; set; }
    }
}
