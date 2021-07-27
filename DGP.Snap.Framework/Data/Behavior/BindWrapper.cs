using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DGP.Snap.Framework.Data.Behavior
{
    public class BindWrapper<K, V>
    {
        private readonly ObservableCollection<BindEntry<K, V>> List = new ObservableCollection<BindEntry<K, V>>();

        public BindWrapper(Dictionary<K, V> dictionary)
        {
            foreach (KeyValuePair<K, V> entry in dictionary)
            {
                this.List.Add(new BindEntry<K, V>(entry.Key, entry.Value));
            }
        }
    }

    public class BindEntry<K, V>
    {
        public BindEntry(K key, V value)
        {
            this.Key = key;
            this.Value = value;
        }
        public K Key { get; set; }
        public V Value { get; set; }
    }
}
