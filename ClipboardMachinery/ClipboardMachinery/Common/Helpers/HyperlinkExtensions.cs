using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace ClipboardMachinery.Common.Helpers {

    public static class HyperlinkExtensions {

        #region Properties

        public static bool GetIsExternal(DependencyObject obj) {
            return (bool)obj.GetValue(IsExternalProperty);
        }

        public static void SetIsExternal(DependencyObject obj, bool value) {
            obj.SetValue(IsExternalProperty, value);
        }

        public static readonly DependencyProperty IsExternalProperty = DependencyProperty.RegisterAttached(
            name: "IsExternal",
            propertyType: typeof(bool),
            ownerType: typeof(HyperlinkExtensions),
            defaultMetadata: new UIPropertyMetadata(false, OnIsExternalChanged)
        );

        #endregion

        #region Handlers

        private static void OnIsExternalChanged(object sender, DependencyPropertyChangedEventArgs args) {
            if (!(sender is Hyperlink hyperlink)) {
                return;
            }

            if ((bool)args.NewValue) {
                hyperlink.RequestNavigate += OnExternalNavigationRequest;
            } else {
                hyperlink.RequestNavigate -= OnExternalNavigationRequest;
            }
        }

        private static void OnExternalNavigationRequest(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        #endregion

    }

}
