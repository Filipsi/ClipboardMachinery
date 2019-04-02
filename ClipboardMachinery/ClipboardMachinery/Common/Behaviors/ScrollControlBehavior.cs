﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ClipboardMachinery.Common.Behaviors {

    internal class ScrollControlBehavior : Behavior<ScrollViewer> {

        #region Properties

        public static readonly DependencyProperty RemainingScrollableHeightProperty = DependencyProperty.Register(
            name: nameof(RemainingScrollableHeight),
            propertyType: typeof(double),
            ownerType: typeof(ScrollControlBehavior)
        );

        public double RemainingScrollableHeight {
            get { return (double)GetValue(RemainingScrollableHeightProperty); }
            set { SetValue(RemainingScrollableHeightProperty, value); }
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(
            name: nameof(VerticalOffset),
            propertyType: typeof(double),
            ownerType: typeof(ScrollControlBehavior),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: 0D,
                flags: FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                propertyChangedCallback: OnVerticalOffsetChange
            )
        );

        public double VerticalOffset {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        #endregion

        #region Behavior

        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.ScrollChanged += OnScrollChanged;
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            AssociatedObject.ScrollChanged -= OnScrollChanged;
        }

        #endregion

        #region Handlers

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e) {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            RemainingScrollableHeight = scrollViewer.ScrollableHeight - scrollViewer.VerticalOffset;
            VerticalOffset = scrollViewer.VerticalOffset;
        }

        private static void OnVerticalOffsetChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ScrollControlBehavior behavior = d as ScrollControlBehavior;
            behavior.AssociatedObject.ScrollToVerticalOffset((double)e.NewValue);
        }

        #endregion

    }

}