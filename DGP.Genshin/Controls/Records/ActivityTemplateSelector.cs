using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls.Records
{
    public class ActivityTemplateSelector : DataTemplateSelector
    {
        private static readonly Dictionary<string, string> TemplateDict = new Dictionary<string, string>
        {
            { "activities[0]", "Effigy" },
            { "activities[1]", "Mechanicus" },
            { "activities[2]", "FleurFair" },
            { "activities[3]", "ChannellerSlab" },
            { "activities[4]", "MartialLegend" },
            { "activities[5]", "Chess" },
            { "activities[6]", "Sumo" }
        };

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element && item != null)
            {
                JToken token = item as JToken;
                string path = token.Path;
                if (TemplateDict.ContainsKey(path))
                {
                    return element.FindResource(TemplateDict[path]) as DataTemplate;
                }
                else
                {
                    return element.FindResource("Default") as DataTemplate;
                }
                
            }
            return null;
        }
    }
}
