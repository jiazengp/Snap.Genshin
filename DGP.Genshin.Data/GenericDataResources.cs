using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin.Data
{
    public class GenericDataResources : ResourceDictionary
    {
        public GenericDataResources()
        {
            MergedDictionaries.Add(GenericDictionary);
        }

        private static ResourceDictionary genericDictionary;
        internal static ResourceDictionary GenericDictionary
        {
            get
            {
                if (genericDictionary == null)
                {
                    genericDictionary = new ResourceDictionary { Source = new Uri($"pack://application:,,,/DGP.Genshin.Data;component/Generic.xaml") };
                }
                return genericDictionary;
            }
        }
    }
}
