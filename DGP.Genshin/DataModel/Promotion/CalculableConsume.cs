using DGP.Genshin.MiHoYoAPI.Calculation;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Input;

namespace DGP.Genshin.DataModel.Promotion
{
    public record CalculableConsume 
    {
        public CalculableConsume(Calculable calculable, List<ConsumeItem> consumption)
        {
            Calculable = calculable;
            ConsumeItems = consumption;
        }

        public Calculable Calculable { get; set; }
        public List<ConsumeItem> ConsumeItems { get; set; }
        [JsonIgnore] public ICommand? RemoveCommand { get; set; }
    }
}
