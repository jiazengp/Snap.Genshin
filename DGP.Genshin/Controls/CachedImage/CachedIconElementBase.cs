using ModernWpf.Controls;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace DGP.Genshin.Controls.CachedImage
{
    [TypeConverter(typeof(IconElementConverter))]
    public abstract class CachedIconElementBase : FrameworkElement
    {
        private protected CachedIconElementBase()
        {
        }

        #region Foreground

        /// <summary>
        /// Identifies the Foreground dependency property.
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty =
                TextElement.ForegroundProperty.AddOwner(
                        typeof(CachedIconElementBase),
                        new FrameworkPropertyMetadata(SystemColors.ControlTextBrush,
                            FrameworkPropertyMetadataOptions.Inherits,
                            OnForegroundPropertyChanged));

        private static void OnForegroundPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) => ((CachedIconElementBase)sender).OnForegroundPropertyChanged(args);

        private void OnForegroundPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            BaseValueSource baseValueSource = DependencyPropertyHelper.GetValueSource(this, args.Property).BaseValueSource;
            this._isForegroundDefaultOrInherited = baseValueSource <= BaseValueSource.Inherited;
            UpdateShouldInheritForegroundFromVisualParent();
        }

        /// <summary>
        /// Gets or sets a brush that describes the foreground color.
        /// </summary>
        /// <returns>
        /// The brush that paints the foreground of the control.
        /// </returns>
        [Bindable(true), Category("Appearance")]
        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        #endregion

        #region VisualParentForeground

        private static readonly DependencyProperty VisualParentForegroundProperty =
            DependencyProperty.Register(
                nameof(VisualParentForeground),
                typeof(Brush),
                typeof(CachedIconElementBase),
                new PropertyMetadata(null, OnVisualParentForegroundPropertyChanged));

        private protected Brush VisualParentForeground
        {
            get => (Brush)GetValue(VisualParentForegroundProperty);
            set => SetValue(VisualParentForegroundProperty, value);
        }

        private static void OnVisualParentForegroundPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) => ((CachedIconElementBase)sender).OnVisualParentForegroundPropertyChanged(args);

        private protected virtual void OnVisualParentForegroundPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
        }

        #endregion

        private protected bool ShouldInheritForegroundFromVisualParent
        {
            get => this._shouldInheritForegroundFromVisualParent;
            private set
            {
                if (this._shouldInheritForegroundFromVisualParent != value)
                {
                    this._shouldInheritForegroundFromVisualParent = value;

                    if (this._shouldInheritForegroundFromVisualParent)
                    {
                        SetBinding(VisualParentForegroundProperty,
                            new Binding
                            {
                                Path = new PropertyPath(TextElement.ForegroundProperty),
                                Source = VisualParent
                            });
                    }
                    else
                    {
                        ClearValue(VisualParentForegroundProperty);
                    }

                    OnShouldInheritForegroundFromVisualParentChanged();
                }
            }
        }

        private protected virtual void OnShouldInheritForegroundFromVisualParentChanged()
        {
        }

        private void UpdateShouldInheritForegroundFromVisualParent()
        {
            this.ShouldInheritForegroundFromVisualParent =
                this._isForegroundDefaultOrInherited &&
                this.Parent != null &&
                this.VisualParent != null &&
                this.Parent != this.VisualParent;
        }

        private protected UIElementCollection Children
        {
            get
            {
                EnsureLayoutRoot();
                return this._layoutRoot.Children;
            }
        }

        private protected abstract void InitializeChildren();

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0)
            {
                EnsureLayoutRoot();
                return this._layoutRoot;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            EnsureLayoutRoot();
            this._layoutRoot.Measure(availableSize);
            return this._layoutRoot.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            EnsureLayoutRoot();
            this._layoutRoot.Arrange(new Rect(new Point(), finalSize));
            return finalSize;
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            UpdateShouldInheritForegroundFromVisualParent();
        }

        private void EnsureLayoutRoot()
        {
            if (this._layoutRoot != null)
                return;

            this._layoutRoot = new Grid
            {
                Background = Brushes.Transparent,
                SnapsToDevicePixels = true,
            };
            InitializeChildren();

            AddVisualChild(this._layoutRoot);
        }

        private Grid _layoutRoot;
        private bool _isForegroundDefaultOrInherited = true;
        private bool _shouldInheritForegroundFromVisualParent;
    }
}
