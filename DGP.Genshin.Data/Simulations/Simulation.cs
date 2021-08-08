using DGP.Snap.Framework.Data.Behavior;

namespace DGP.Genshin.Data.Simulations
{
    public class Simulation : Observable
    {
        private Simulator simulator;
        public Simulator Simulator { get => simulator; set => Set(ref simulator, value); }
    }
}
