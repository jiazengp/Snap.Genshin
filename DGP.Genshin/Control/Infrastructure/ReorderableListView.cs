using System.Collections.Generic;
using System.Windows.Media;

namespace DGP.Genshin.Control.Infrastructure
{
    /// <summary>
    /// 可排序的列表视图
    /// BUG非常多
    /// </summary>
    /// <typeparam name="TItem">项类型</typeparam>
    public abstract class ReorderableListView<TItem> : ModernWpf.Controls.ListView
        where TItem : class
    {
        // Handling the reordering
        private UIElement? selectedElementContainer = null;

        /// <summary>
        /// 构造一个新的可排序的列表视图
        /// </summary>
        public ReorderableListView()
            : base()
        {
            AllowDrop = true;
            PreviewMouseLeftButtonDown += OnElementClicked_TryTake;
            PreviewMouseMove += OnDrag_MoveElement;
            PreviewMouseLeftButtonUp += OnElementRelease_LeaveElement;
            Drop += OnDrop_TryPlaceElement;
        }

        /// <summary>
        /// After drop
        /// </summary>
        /// <param name="item">项</param>
        public virtual void AfterDrop(TItem item)
        {
        }

        private void OnElementClicked_TryTake(object sender, MouseButtonEventArgs e)
        {
            Point mouseInListBoxPosition = e.GetPosition(this);
            selectedElementContainer = GetElementAtPosition(mouseInListBoxPosition);
        }

        private void OnDrag_MoveElement(object sender, MouseEventArgs e)
        {
            if (selectedElementContainer == null)
            {
                return;
            }

            Point mouseInListBoxPosition = e.GetPosition(this);
            UIElement? hoverElementContainer = GetElementAtPosition(mouseInListBoxPosition);
            if (hoverElementContainer == selectedElementContainer)
            {
                return;
            }

            TItem? draggedItem = (TItem)ItemContainerGenerator.ItemFromContainer(selectedElementContainer);
            SelectedItem = draggedItem;

            selectedElementContainer = null;
            ReorderableListView<TItem>.DragData? dragData = new(this, draggedItem);
            DragDrop.DoDragDrop(this, new DataObject(typeof(DragData), dragData), DragDropEffects.Move);
        }

        private void OnElementRelease_LeaveElement(object sender, MouseButtonEventArgs e)
        {
            selectedElementContainer = null;
        }

        private void OnDrop_TryPlaceElement(object sender, DragEventArgs e)
        {
            ReorderableListView<TItem>.DragData? dragData = e.Data.GetData(typeof(DragData)) as DragData;
            if (dragData == null)
            {
                return;
            }

            Point mouseInListBoxPosition = e.GetPosition(this);
            UIElement? hoverElementContainer = GetElementAtPosition(mouseInListBoxPosition);
            object? hoverElement = hoverElementContainer != null
                ? ItemContainerGenerator.ItemFromContainer(hoverElementContainer)
                : null;

            TItem? droppedItem = dragData.Item;
            if (object.Equals(hoverElement, droppedItem))
            {
                return;
            }

            ReorderableListView<TItem>? sourceListBox = dragData.SourceList;
            IList<TItem>? source = (IList<TItem>)sourceListBox.ItemsSource;
            int originalIndex = source.IndexOf(droppedItem);
            source.Remove(droppedItem);

            IList<TItem>? destination = (IList<TItem>)ItemsSource;
            if (hoverElement == null)
            {
                destination.Add(droppedItem);
            }
            else
            {
                int hoverElementIndex = Items.IndexOf(hoverElement);
                bool isMouseAtBottomHalf = e.GetPosition(hoverElementContainer).Y >= ((FrameworkElement)hoverElementContainer!).ActualHeight / 2;
                int insertIndex = hoverElementIndex + (isMouseAtBottomHalf ? 1 : 0);
                insertIndex -= (source == destination && insertIndex >= originalIndex) ? 1 : 0;
                if (insertIndex >= 0)
                {
                    destination.Insert(insertIndex, droppedItem);
                }
            }

            AfterDrop(droppedItem);
        }

        private UIElement? GetElementAtPosition(Point point)
        {
            UIElement? container = VisualTreeHelper.HitTest(this, point)?.VisualHit as UIElement;
            if (container == null)
            {
                return null;
            }

            object item = ItemContainerGenerator.ItemFromContainer(container);

            // search for UI element that directly corresponds to the ListBox item
            while (item == DependencyProperty.UnsetValue)
            {
                container = VisualTreeHelper.GetParent(container) as UIElement;
                if (container == this)
                {
                    return null;
                }

                item = ItemContainerGenerator.ItemFromContainer(container);
            }

            return container;
        }

        private class DragData
        {
            /// <summary>
            /// 构造一个新的拖动数据
            /// </summary>
            /// <param name="sourceList">列表</param>
            /// <param name="item">项</param>
            public DragData(ReorderableListView<TItem> sourceList, TItem item)
            {
                SourceList = sourceList;
                Item = item;
            }

            public ReorderableListView<TItem> SourceList { get; }

            public TItem Item { get; }
        }
    }
}