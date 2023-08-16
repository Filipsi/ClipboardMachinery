using System.Windows;
using System.Windows.Controls;

namespace ClipboardMachinery.Common.Helpers {

    public static class FocusManager {

        #region Properties

        public static readonly DependencyProperty FocusOnLoad = DependencyProperty.RegisterAttached(
            name: "FocusOnLoad",
            propertyType: typeof(bool),
            ownerType: typeof(FocusManager),
            defaultMetadata: new UIPropertyMetadata(
                defaultValue: false,
                propertyChangedCallback: OnValueChanged
            )
        );

        #endregion

        static FocusManager() {
        }

        #region Logic

        public static bool GetFocusOnLoad(DependencyObject d) {
            return (bool)d.GetValue(FocusOnLoad);
        }

        public static void SetFocusOnLoad(DependencyObject d, bool value) {
            d.SetValue(FocusOnLoad, value);
        }

        #endregion

        #region Handlers

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            if (sender is not Control control) {
                return;
            }

            if (e.NewValue is not bool value) {
                return;
            }

            if (value && !control.IsLoaded) {
                control.Loaded += OnControlLoaded;
            } else {
                control.Loaded -= OnControlLoaded;
            }
        }

        private static void OnControlLoaded(object sender, RoutedEventArgs e) {
            if (sender is not Control element) {
                return;
            }

            element.Loaded -= OnControlLoaded;

            switch (element) {
                case TextBox textBox:
                    textBox.SelectAll();
                    break;

                case ListBox listBox when listBox.Items.Count > 0: {
                    listBox.SelectedIndex = 0;
                    break;
                }
            }

            element.Focus();
        }

        #endregion

    }

}
