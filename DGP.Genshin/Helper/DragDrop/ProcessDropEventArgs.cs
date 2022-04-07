using System.Collections.ObjectModel;

namespace DGP.Genshin.Helper.DragDrop
{
    /// <summary>
    /// Event arguments used by the ListViewDragDropManager.ProcessDrop event.
    /// </summary>
    /// <typeparam name="TItemType">The type of data object being dropped.</typeparam>
    [SuppressMessage("", "SA1600")]
    public class ProcessDropEventArgs<TItemType> : EventArgs
        where TItemType : class
    {
        private readonly ObservableCollection<TItemType> itemsSource;
        private readonly TItemType dataItem;
        private readonly int oldIndex;
        private readonly int newIndex;
        private readonly DragDropEffects allowedEffects = DragDropEffects.None;
        private DragDropEffects effects = DragDropEffects.None;

        internal ProcessDropEventArgs(
            ObservableCollection<TItemType> itemsSource,
            TItemType dataItem,
            int oldIndex,
            int newIndex,
            DragDropEffects allowedEffects)
        {
            this.itemsSource = itemsSource;
            this.dataItem = dataItem;
            this.oldIndex = oldIndex;
            this.newIndex = newIndex;
            this.allowedEffects = allowedEffects;
        }

        /// <summary>
        /// The items source of the ListView where the drop occurred.
        /// </summary>
        public ObservableCollection<TItemType> ItemsSource
        {
            get => itemsSource;
        }

        /// <summary>
        /// The data object which was dropped.
        /// </summary>
        public TItemType DataItem
        {
            get => dataItem;
        }

        /// <summary>
        /// The current index of the data item being dropped, in the ItemsSource collection.
        /// </summary>
        public int OldIndex
        {
            get => oldIndex;
        }

        /// <summary>
        /// The target index of the data item being dropped, in the ItemsSource collection.
        /// </summary>
        public int NewIndex
        {
            get => newIndex;
        }

        /// <summary>
        /// The drag drop effects allowed to be performed.
        /// </summary>
        public DragDropEffects AllowedEffects
        {
            get => allowedEffects;
        }

        /// <summary>
        /// The drag drop effect(s) performed on the dropped item.
        /// </summary>
        public DragDropEffects Effects
        {
            get => effects;
            set => effects = value;
        }
    }
}