using DGP.Genshin.ViewModel;

namespace DGP.Genshin.Message.Internal
{
    internal class SplashInitializationCompletedMessage : TypedMessage<SplashViewModel>
    {
        public SplashInitializationCompletedMessage(SplashViewModel viewModel) : base(viewModel)
        {
        }
    }
}
