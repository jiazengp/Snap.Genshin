using System;

namespace DGP.Genshin.Messages
{
    public class NavigateRequestMessage : TypedMessage<Type>
    {
        public NavigateRequestMessage(Type pageType, bool isSyncTabRequested = false) : base(pageType)
        {
            IsSyncTabRequested = isSyncTabRequested;
        }

        public bool IsSyncTabRequested { get; set; }
    }
}
