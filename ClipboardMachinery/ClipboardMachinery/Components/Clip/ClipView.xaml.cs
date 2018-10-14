using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ClipboardMachinery.Components.Clip {

    public partial class ClipView : UserControl {

        private DoubleAnimation expandAnimation = new DoubleAnimation {
            From = 1,
            To = 24,
            Duration = new Duration(TimeSpan.FromMilliseconds(250))
        };

        private DoubleAnimation colapseAnimation = new DoubleAnimation {
            From = 24,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(250))
        };

        private Timer expandTimer = new Timer {
            Interval = 800,
            AutoReset = false,
            Enabled = false
        };

        private bool isExpanded;

        public ClipView() {
            InitializeComponent();
            expandTimer.Elapsed += OnExpandTimerElapsed;
        }

        private void OnLoaded(object sender, EventArgs e) {
            if (DataContext is INotifyPropertyChanged observable) {
                observable.PropertyChanged += OnDataContextPropertyChanged;
            }
        }

        private void OnExpandTimerElapsed(object sender, ElapsedEventArgs e) {
            Dispatcher.Invoke(() => {
                TagPanel.BeginAnimation(HeightProperty, expandAnimation);
                isExpanded = true;
            });
        }

        private void OnDataContextPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != "IsFocused") {
                return;
            }

            bool isFocused = (bool)sender.GetType().GetProperty("IsFocused").GetValue(sender);
            expandTimer.Enabled = isFocused;

            if (!isFocused && isExpanded) {
                TagPanel.BeginAnimation(HeightProperty, colapseAnimation);
                isExpanded = false;
            }
        }

    }

}
