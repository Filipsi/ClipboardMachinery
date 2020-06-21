using System.Windows;
using System.Windows.Media;

namespace ClipboardMachinery.Common.Helpers {

    /// <summary>
    /// Helpers for working with XAML visual tree.
    /// </summary>
    public static class VisualTreeExtensions {

        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject {
            DependencyObject previousParent = child;

            do {
                previousParent = VisualTreeHelper.GetParent(previousParent);
                if (previousParent is T parent) {
                    return parent;
                }
            } while (previousParent != null);

            return null;
        }

    }
}
