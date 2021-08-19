using DGP.Snap.Framework.Data.Behavior;
using System;

namespace DGP.Genshin.Data.Simulations
{
    [Obsolete]
    public class Simulation : Observable
    {
        private Simulator simulator;
        public Simulator Simulator { get => this.simulator; set => Set(ref this.simulator, value); }
    }
}
