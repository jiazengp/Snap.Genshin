using System;
using System.Collections.ObjectModel;

namespace DGP.Genshin.Data.Simulations
{
    [Obsolete]
    public class SimulationCollection
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ObservableCollection<Simulation> Collections { get; set; } = new ObservableCollection<Simulation>();
    }
}
