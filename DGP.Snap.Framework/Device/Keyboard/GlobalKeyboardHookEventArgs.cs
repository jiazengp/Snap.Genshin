using System.ComponentModel;

namespace DGP.Snap.Framework.Device.Keyboard
{
    public class GlobalKeyboardHookEventArgs : HandledEventArgs
    {
        public KeyboardState KeyboardState { get; private set; }
        public LowLevelKeyboardInputEvent KeyboardData { get; private set; }

        public GlobalKeyboardHookEventArgs(
            LowLevelKeyboardInputEvent keyboardData,
            KeyboardState keyboardState)
        {
            this.KeyboardData = keyboardData;
            this.KeyboardState = keyboardState;
        }
    }
}
