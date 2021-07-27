using DGP.Snap.Framework.Data.Behavior;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DGP.Snap.Framework.Extensions.System.Collections.ObjectModel
{
    public static class ObservableCollectionExtensions
    {
        public static ObservableCollection<Selectable<T>> ToObservableSelectable<T>(this ObservableCollection<T> collection, bool isSelected = false, Action<bool> onSelectChanged = null) => new ObservableCollection<Selectable<T>>(collection.Select(c => new Selectable<T>(c, isSelected, onSelectChanged)));

        public static IEnumerable<T> ToSelected<T>(this ObservableCollection<Selectable<T>> collection, bool isSelected = false, Action<bool> onSelectChanged = null) => collection.Where(i => i.IsSelected).Select(i => i.Value);
    }
}
