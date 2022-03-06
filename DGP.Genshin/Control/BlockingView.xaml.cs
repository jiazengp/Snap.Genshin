using DGP.Genshin.Message;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Control
{
    public sealed partial class BlockingView : UserControl, IRecipient<ImageHitBeginMessage>, IRecipient<ImageHitEndMessage>
    {
        public BlockingView()
        {
            DataContext = this;
            InitializeComponent();

            App.Messenger.RegisterAll(this);
        }
        ~BlockingView()
        {
            App.Messenger.UnregisterAll(this);
        }

        public bool ShouldPresent
        {
            get => (bool)GetValue(ShouldPresentProperty);

            set => SetValue(ShouldPresentProperty, value);
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
