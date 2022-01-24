using DGP.Genshin.ViewModel;

namespace DGP.Genshin.Message
{
    public class SplashInitializationCompletedMessage : TypedMessage<SplashViewModel>
    {
        public SplashInitializationCompletedMessage(SplashViewModel viewModel) : base(viewModel)
        {
        }
    }
}
