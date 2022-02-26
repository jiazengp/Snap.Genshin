using DGP.Genshin.Message;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Control
{
    public partial class BlockingView : UserControl, IRecipient<ImageHitBeginMessage>, IRecipient<ImageHitEndMessage>
    {
        public BlockingView()
        {
            DataContext = this;
            InitializeComponent();

            App.Messenger.Register<ImageHitBeginMessage>(this);
            App.Messenger.Register<ImageHitEndMessage>(this);
        }
        ~BlockingView()
        {
            App.Messenger.Unregister<ImageHitBeginMessage>(this);
            App.Messenger.Unregister<ImageHitEndMessage>(this);
        }

        public bool ShouldPresent
        {
            get { return (bool)GetValue(ShouldPresentProperty); }
            set { SetValue(ShouldPresentProperty, value); }
        }
        public static readonly DependencyProperty ShouldPresentProperty =
            DependencyProperty.Register(nameof(ShouldPresent), typeof(bool), typeof(BlockingView), new PropertyMetadata(false));

        public void Receive(ImageHitBeginMessage message)
        {
            ShouldPresent = true;
        }
        public void Receive(ImageHitEndMessage message)
        {
            ShouldPresent = false;
        }
    }
}
