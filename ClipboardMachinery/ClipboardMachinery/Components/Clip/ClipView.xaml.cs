using System;
using System.ComponentModel;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Media.Animation;

namespace ClipboardMachinery.Components.Clip {

    public partial class ClipView {

        #region Fields

        private readonly DoubleAnimation expandAnimation = new DoubleAnimation {
            From = 1,
            To = 32,
            Duration = new Duration(TimeSpan.FromMilliseconds(250))
        };

        private readonly DoubleAnimation colapseAnimation = new DoubleAnimation {
            From = 32,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(250))
        };

        private readonly Timer expandTimer = new Timer {
            Interval = 800,
            AutoReset = false,
            Enabled = false
        };

        private bool isExpanded;

        #endregion

        public ClipView() {
            InitializeComponent();
        }

        #region Handlers

        private void OnLoaded(object sender, EventArgs e) {
            expandTimer.Elapsed += OnExpandTimerElapsed;

            if (DataContext is INotifyPropertyChanged observable) {
                observable.PropertyChanged += OnDataContextPropertyChanged;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            expandTimer.Elapsed -= OnExpandTimerElapsed;

            if (DataContext is INotifyPropertyChanged observable) {
                observable.PropertyChanged -= OnDataContextPropertyChanged;
            }
        }

        private void OnExpandTimerElapsed(object sender, ElapsedEventArgs e) {
            Application.Current.Dispatcher?.Invoke(() => {
                TagPanel.BeginAnimation(HeightProperty, expandAnimation);
                isExpanded = true;
            });
        }

        private void OnDataContextPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != "IsFocused") {
                return;
            }

            PropertyInfo isFocusedProperty = sender.GetType().GetProperty("IsFocused");
            if (isFocusedProperty == null) {
                return;
            }

            bool isFocused = (bool)isFocusedProperty.GetValue(sender);
            expandTimer.Enabled = isFocused;

            if (isFocused || !isExpanded) {
                return;
            }

            TagPanel.BeginAnimation(HeightProperty, colapseAnimation);
            isExpanded = false;
        }

        #endregion

    }

}
