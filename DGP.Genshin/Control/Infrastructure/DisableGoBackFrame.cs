namespace DGP.Genshin.Control.Infrastructure
{
    public class DisableGoBackFrame : ModernWpf.Controls.Frame
    {
        public DisableGoBackFrame()
        {
            //remove all command bindings to prevent hotkey go back.
            CommandBindings.Clear();
        }
    }
}
