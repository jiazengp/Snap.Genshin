using DGP.Genshin.Data.Simulations;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace DGP.Genshin.Services
{
    public class SimulationService : Observable
    {
        private const string simulationsFileName = "simulations.json";
        private readonly string simulationsFile = AppDomain.CurrentDomain.BaseDirectory + simulationsFileName;

        private SimulationCollection selectedSimulationCollection;
        public SimulationCollection SelectedSimulationCollection { get => selectedSimulationCollection; set => Set(ref selectedSimulationCollection, value); }
        public ObservableCollection<SimulationCollection> SimulationCollections { get; set; }

        public void Initialize()
        {
            if (File.Exists(this.simulationsFile))
            {
                string json;
                using (StreamReader sr = new(this.simulationsFile))
                {
                    json = sr.ReadToEnd();
                }
                ObservableCollection<SimulationCollection> collections = Json.ToObject<ObservableCollection<SimulationCollection>>(json);
                if (collections != null)
                {
                    this.SimulationCollections = collections;
                    return;
                }
            }
            File.Create(this.simulationsFile).Dispose();
            this.SimulationCollections = new ObservableCollection<SimulationCollection>();
        }
        public void UnInitialize()
        {
            string json = Json.Stringify(this.SimulationCollections);
            using StreamWriter sw = new StreamWriter(File.Create(this.simulationsFile));
            sw.Write(json);
        }

        public SimulationService()
        {
            this.Log("SimulationService Initialized");
            this.Initialize();
        }
        ~SimulationService()
        {
            UnInitialize();
        }
    }
}
