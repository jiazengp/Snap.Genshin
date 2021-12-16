using DGP.Genshin.ViewModels;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace DGP.Genshin.Messages
{
    public class SplashInitializationCompletedMessage : ValueChangedMessage<SplashViewModel>
    {
        public SplashInitializationCompletedMessage(SplashViewModel viewModel) : base(viewModel)
        {
        }
    }
}
