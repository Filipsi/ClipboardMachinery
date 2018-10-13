using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ClipboardMachinery.Common.Behaviors {

    internal class RemainingScrollableHeightBehavior : Behavior<ScrollViewer> {

        #region Properties

        public static readonly DependencyProperty RemainingScrollableHeightProperty = DependencyProperty.Register(
            name: nameof(RemainingScrollableHeight),
            propertyType: typeof(object),
            ownerType: typeof(RemainingScrollableHeightBehavior)
        );

        public double RemainingScrollableHeight {
            get { return (double)GetValue(RemainingScrollableHeightProperty); }
            set { SetValue(RemainingScrollableHeightProperty, value); }
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
        }

        #endregion

    }

}
