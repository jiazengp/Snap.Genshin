using DGP.Genshin.ViewModels;

namespace DGP.Genshin.Messages
{
    public class SplashInitializationCompletedMessage : TypedMessage<SplashViewModel>
    {
        public SplashInitializationCompletedMessage(SplashViewModel viewModel) : base(viewModel)
        {
        }
    }
}
