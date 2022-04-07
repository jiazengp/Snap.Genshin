using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace DGP.Genshin.Helper.DragDrop
{
    /// <summary>
    /// Manages the dragging and dropping of ListViewItems in a ListView.
    /// The ItemType type parameter indicates the type of the objects in
    /// the ListView's items source.  The ListView's ItemsSource must be
    /// set to an instance of ObservableCollection of ItemType, or an
    /// Exception will be thrown.
    /// </summary>
    /// <typeparam name="TItemType">The type of the ListView's items.</typeparam>
    [SuppressMessage("", "SA1201")]
    [SuppressMessage("", "SA1614")]
    public class ListViewDragDropManager<TItemType>
        where TItemType : class
    {
        private bool canInitiateDrag;
        private DragAdorner? dragAdorner;
        private double dragAdornerOpacity;
        private int indexToSelect;
        private bool isDragInProgress;
        private TItemType? itemUnderDragCursor;
        private ListView listView;
        private Point ptMouseDown;
        private bool showDragAdorner;

        /// <summary>
        /// Initializes a new instance of ListViewDragManager.
        /// </summary>
        /// <param name="listView"></param>
        public ListViewDragDropManager(ListView listView)
        {
            canInitiateDrag = false;
            dragAdornerOpacity = 0.7;
            indexToSelect = -1;
            showDragAdorner = true;
            ListView = listView;
        }

        /// <summary>
        /// Initializes a new instance of ListViewDragManager.
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="dragAdornerOpacity"></param>
        public ListViewDragDropManager(ListView listView, double dragAdornerOpacity)
            : this(listView)
        {
            DragAdornerOpacity = dragAdornerOpacity;
        }

        /// <summary>
        /// Initializes a new instance of ListViewDragManager.
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="showDragAdorner"></param>
        public ListViewDragDropManager(ListView listView, bool showDragAdorner)
            : this(listView)
        {
            ShowDragAdorner = showDragAdorner;
        }

        /// <summary>
        /// Gets/sets the opacity of the drag adorner.  This property has no
        /// effect if ShowDragAdorner is false. The default value is 0.7
        /// </summary>
        public double DragAdornerOpacity
        {
            get => dragAdornerOpacity;
            set
            {
                if (IsDragInProgress)
                {
                    throw new InvalidOperationException("Cannot set the DragAdornerOpacity property during a drag operation.");
                }

                if (value < 0.0 || value > 1.0)
                {
                    throw new ArgumentOutOfRangeException("DragAdornerOpacity", value, "Must be between 0 and 1.");
                }

                dragAdornerOpacity = value;
            }
        }

        /// <summary>
        /// Returns true if there is currently a drag operation being managed.
        /// </summary>
        public bool IsDragInProgress
        {
            get => isDragInProgress;
            private set => isDragInProgress = value;
        }

        /// <summary>
        /// Gets/sets the ListView whose dragging is managed.  This property
        /// can be set to null, to prevent drag management from occuring.  If
        /// the ListView's AllowDrop property is false, it will be set to true.
        /// </summary>
        public ListView ListView
        {
            get => listView;
            [MemberNotNull(nameof(listView))]
            set
            {
                if (IsDragInProgress)
                {
                    throw new InvalidOperationException("Cannot set the ListView property during a drag operation.");
                }

                if (listView != null)
                {
                    listView.PreviewMouseLeftButtonDown -= ListViewPreviewMouseLeftButtonDown;
                    listView.PreviewMouseMove -= ListViewPreviewMouseMove;
                    listView.DragOver -= ListViewDragOver;
                    listView.DragLeave -= ListViewDragLeave;
                    listView.DragEnter -= ListViewDragEnter;
                    listView.Drop -= ListViewDrop;
                }

                listView = value;

                if (listView != null)
                {
                    if (!listView.AllowDrop)
                    {
                        listView.AllowDrop = true;
                    }

                    listView.PreviewMouseLeftButtonDown += ListViewPreviewMouseLeftButtonDown;
                    listView.PreviewMouseMove += ListViewPreviewMouseMove;
                    listView.DragOver += ListViewDragOver;
                    listView.DragLeave += ListViewDragLeave;
                    listView.DragEnter += ListViewDragEnter;
                    listView.Drop += ListViewDrop;
                }

                Must.NotNull(listView!);
            }
        }

        /// <summary>
        /// Raised when a drop occurs.  By default the dropped item will be moved
        /// to the target index.  Handle this event if relocating the dropped item
        /// requires custom behavior.  Note, if this event is handled the default
        /// item dropping logic will not occur.
        /// </summary>
        public event EventHandler<ProcessDropEventArgs<TItemType>>? ProcessDrop;

        /// <summary>
        /// Gets/sets whether a visual representation of the ListViewItem being dragged
        /// follows the mouse cursor during a drag operation.  The default value is true.
        /// </summary>
        public bool ShowDragAdorner
        {
            get => showDragAdorner;
            set
            {
                if (IsDragInProgress)
                {
                    throw new InvalidOperationException("Cannot set the ShowDragAdorner property during a drag operation.");
                }

                showDragAdorner = value;
            }
        }

        private void ListViewPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseOverScrollbar)
            {
                // 4/13/2007 - Set the flag to false when cursor is over scrollbar.
                canInitiateDrag = false;
                return;
            }

            int index = IndexUnderDragCursor;
            canInitiateDrag = index > -1;

            if (canInitiateDrag)
            {
                // Remember the location and index of the ListViewItem the user clicked on for later.
                ptMouseDown = MouseUtilities.GetMousePosition(listView);
                indexToSelect = index;
            }
            else
            {
                ptMouseDown = new Point(-10000, -10000);
                indexToSelect = -1;
            }
        }

        private void ListViewPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!CanStartDragOperation)
            {
                return;
            }

            // Select the item the user clicked on.
            if (listView.SelectedIndex != indexToSelect)
            {
                listView.SelectedIndex = indexToSelect;
            }

            // If the item at the selected index is null, there's nothing
            // we can do, so just return;
            if (listView.SelectedItem == null)
            {
                return;
            }

            ListViewItem? itemToDrag = GetListViewItem(listView.SelectedIndex);
            if (itemToDrag == null)
            {
                return;
            }

            AdornerLayer? adornerLayer = ShowDragAdornerResolved ? InitializeAdornerLayer(itemToDrag) : null;

            InitializeDragOperation(itemToDrag);
            PerformDragOperation();
            FinishDragOperation(itemToDrag, adornerLayer);
        }

        private void ListViewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;

            if (ShowDragAdornerResolved)
            {
                UpdateDragAdornerLocation();
            }

            // Update the item which is known to be currently under the drag cursor.
            int index = IndexUnderDragCursor;
            ItemUnderDragCursor = index < 0 ? null : ListView.Items[index] as TItemType;
        }

        private void ListViewDragLeave(object sender, DragEventArgs e)
        {
            if (!IsMouseOver(listView))
            {
                if (ItemUnderDragCursor != null)
                {
                    ItemUnderDragCursor = null;
                }

                if (dragAdorner != null)
                {
                    dragAdorner.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ListViewDragEnter(object sender, DragEventArgs e)
        {
            if (dragAdorner != null && dragAdorner.Visibility != Visibility.Visible)
            {
                // Update the location of the adorner and then show it.
                UpdateDragAdornerLocation();
                dragAdorner.Visibility = Visibility.Visible;
            }
        }

        private void ListViewDrop(object sender, DragEventArgs e)
        {
            if (ItemUnderDragCursor != null)
            {
                ItemUnderDragCursor = null;
            }

            e.Effects = DragDropEffects.None;

            if (!e.Data.GetDataPresent(typeof(TItemType)))
            {
                return;
            }

            // Get the data object which was dropped.
            if (e.Data.GetData(typeof(TItemType)) is not TItemType data)
            {
                return;
            }

            // Get the ObservableCollection<ItemType> which contains the dropped data object.
            ObservableCollection<TItemType>? itemsSource = listView.ItemsSource as ObservableCollection<TItemType>;

            // A ListView managed by ListViewDragManager must have its ItemsSource set to an ObservableCollection<ItemType>.
            Must.NotNull(itemsSource!);

            int oldIndex = itemsSource.IndexOf(data);
            int newIndex = IndexUnderDragCursor;

            if (newIndex < 0)
            {
                // The drag started somewhere else, and our ListView is empty
                // so make the new item the first in the list.
                if (itemsSource.Count == 0)
                {
                    newIndex = 0;
                }

                // The drag started somewhere else, but our ListView has items
                // so make the new item the last in the list.
                else if (oldIndex < 0)
                {
                    newIndex = itemsSource.Count;
                }

                // The user is trying to drop an item from our ListView into
                // our ListView, but the mouse is not over an item, so don't
                // let them drop it.
                else
                {
                    return;
                }
            }

            // Dropping an item back onto itself is not considered an actual 'drop'.
            if (oldIndex == newIndex)
            {
                return;
            }

            if (ProcessDrop != null)
            {
                // Let the client code process the drop.
                ProcessDropEventArgs<TItemType> args = new(itemsSource, data, oldIndex, newIndex, e.AllowedEffects);
                ProcessDrop(this, args);
                e.Effects = args.Effects;
            }
            else
            {
                // Move the dragged data object from it's original index to the
                // new index (according to where the mouse cursor is).  If it was
                // not previously in the ListBox, then insert the item.
                if (oldIndex > -1)
                {
                    itemsSource.Move(oldIndex, newIndex);
                }
                else
                {
                    itemsSource.Insert(newIndex, data);
                }

                // Set the Effects property so that the call to DoDragDrop will return 'Move'.
                e.Effects = DragDropEffects.Move;
            }
        }

        private bool CanStartDragOperation
        {
            get
            {
                if (Mouse.LeftButton != MouseButtonState.Pressed)
                {
                    return false;
                }

                if (!canInitiateDrag)
                {
                    return false;
                }

                if (indexToSelect == -1)
                {
                    return false;
                }

                if (!HasCursorLeftDragThreshold)
                {
                    return false;
                }

                return true;
            }
        }

        private void FinishDragOperation(ListViewItem draggedItem, AdornerLayer? adornerLayer)
        {
            // Let the ListViewItem know that it is not being dragged anymore.
            ListViewItemDragState.SetIsBeingDragged(draggedItem, false);

            IsDragInProgress = false;

            if (ItemUnderDragCursor != null)
            {
                ItemUnderDragCursor = null;
            }

            // Remove the drag adorner from the adorner layer.
            if (adornerLayer != null)
            {
                adornerLayer.Remove(dragAdorner);
                dragAdorner = null;
            }
        }

        private ListViewItem? GetListViewItem(int index)
        {
            if (listView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                return null;
            }

            return listView.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
        }

        private ListViewItem? GetListViewItem(TItemType dataItem)
        {
            if (listView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                return null;
            }

            return listView.ItemContainerGenerator.ContainerFromItem(dataItem) as ListViewItem;
        }

        private bool HasCursorLeftDragThreshold
        {
            get
            {
                if (indexToSelect < 0)
                {
                    return false;
                }

                ListViewItem? item = GetListViewItem(indexToSelect);
                Rect bounds = VisualTreeHelper.GetDescendantBounds(item);
                Point ptInItem = listView.TranslatePoint(ptMouseDown, item);

                // In case the cursor is at the very top or bottom of the ListViewItem
                // we want to make the vertical threshold very small so that dragging
                // over an adjacent item does not select it.
                double topOffset = Math.Abs(ptInItem.Y);
                double btmOffset = Math.Abs(bounds.Height - ptInItem.Y);
                double vertOffset = Math.Min(topOffset, btmOffset);

                double width = SystemParameters.MinimumHorizontalDragDistance * 2;
                double height = Math.Min(SystemParameters.MinimumVerticalDragDistance, vertOffset) * 2;
                Size szThreshold = new(width, height);

                Rect rect = new(ptMouseDown, szThreshold);
                rect.Offset(szThreshold.Width / -2, szThreshold.Height / -2);
                Point ptInListView = MouseUtilities.GetMousePosition(listView);
                return !rect.Contains(ptInListView);
            }
        }

        /// <summary>
        /// Returns the index of the ListViewItem underneath the
        /// drag cursor, or -1 if the cursor is not over an item.
        /// </summary>
        private int IndexUnderDragCursor
        {
            get
            {
                int index = -1;
                for (int i = 0; i < listView.Items.Count; ++i)
                {
                    ListViewItem? item = GetListViewItem(i);
                    Must.NotNull(item!);
                    if (IsMouseOver(item))
                    {
                        index = i;
                        break;
                    }
                }

                return index;
            }
        }

        private AdornerLayer InitializeAdornerLayer(ListViewItem itemToDrag)
        {
            // Create a brush which will paint the ListViewItem onto
            // a visual in the adorner layer.
            VisualBrush brush = new(itemToDrag);

            // Create an element which displays the source item while it is dragged.
            dragAdorner = new DragAdorner(listView, itemToDrag.RenderSize, brush)
            {
                // Set the drag adorner's opacity.
                Opacity = DragAdornerOpacity,
            };

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(listView);
            layer.Add(dragAdorner);

            // Save the location of the cursor when the left mouse button was pressed.
            ptMouseDown = MouseUtilities.GetMousePosition(listView);

            return layer;
        }

        private void InitializeDragOperation(ListViewItem itemToDrag)
        {
            // Set some flags used during the drag operation.
            IsDragInProgress = true;
            canInitiateDrag = false;

            // Let the ListViewItem know that it is being dragged.
            ListViewItemDragState.SetIsBeingDragged(itemToDrag, true);
        }

        private bool IsMouseOver(Visual target)
        {
            // We need to use MouseUtilities to figure out the cursor
            // coordinates because, during a drag-drop operation, the WPF
            // mechanisms for getting the coordinates behave strangely.
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = MouseUtilities.GetMousePosition(target);
            return bounds.Contains(mousePos);
        }

        /// <summary>
        /// Returns true if the mouse cursor is over a scrollbar in the ListView.
        /// </summary>
        private bool IsMouseOverScrollbar
        {
            get
            {
                Point ptMouse = MouseUtilities.GetMousePosition(listView);
                HitTestResult res = VisualTreeHelper.HitTest(listView, ptMouse);
                if (res == null)
                {
                    return false;
                }

                DependencyObject depObj = res.VisualHit;
                while (depObj != null)
                {
                    if (depObj is ScrollBar)
                    {
                        return true;
                    }

                    // VisualTreeHelper works with objects of type Visual or Visual3D.
                    // If the current object is not derived from Visual or Visual3D,
                    // then use the LogicalTreeHelper to find the parent element.
                    if (depObj is Visual || depObj is System.Windows.Media.Media3D.Visual3D)
                    {
                        depObj = VisualTreeHelper.GetParent(depObj);
                    }
                    else
                    {
                        depObj = LogicalTreeHelper.GetParent(depObj);
                    }
                }

                return false;
            }
        }

        private TItemType? ItemUnderDragCursor
        {
            get => itemUnderDragCursor;
            set
            {
                if (itemUnderDragCursor == value)
                {
                    return;
                }

                // The first pass handles the previous item under the cursor.
                // The second pass handles the new one.
                for (int i = 0; i < 2; ++i)
                {
                    if (i == 1)
                    {
                        itemUnderDragCursor = value;
                    }

                    if (itemUnderDragCursor != null)
                    {
                        ListViewItem? listViewItem = GetListViewItem(itemUnderDragCursor);
                        if (listViewItem != null)
                        {
                            ListViewItemDragState.SetIsUnderDragCursor(listViewItem, i == 1);
                        }
                    }
                }
            }
        }

        private void PerformDragOperation()
        {
            TItemType? selectedItem = listView.SelectedItem as TItemType;
            DragDropEffects allowedEffects = DragDropEffects.Move | DragDropEffects.Move | DragDropEffects.Link;
            if (System.Windows.DragDrop.DoDragDrop(listView, selectedItem, allowedEffects) != DragDropEffects.None)
            {
                // The item was dropped into a new location,
                // so make it the new selected item.
                listView.SelectedItem = selectedItem;
            }
        }

        private bool ShowDragAdornerResolved
        {
            get => ShowDragAdorner && DragAdornerOpacity > 0.0;
        }

        private void UpdateDragAdornerLocation()
        {
            if (dragAdorner != null)
            {
                Point ptCursor = MouseUtilities.GetMousePosition(ListView);

                double left = ptCursor.X - ptMouseDown.X;

                // 4/13/2007 - Made the top offset relative to the item being dragged.
                ListViewItem? itemBeingDragged = GetListViewItem(indexToSelect);
                Must.NotNull(itemBeingDragged!);
                Point itemLoc = itemBeingDragged.TranslatePoint(new Point(0, 0), ListView);
                double top = itemLoc.Y + ptCursor.Y - ptMouseDown.Y;

                dragAdorner.SetOffsets(left, top);
            }
        }
    }
}