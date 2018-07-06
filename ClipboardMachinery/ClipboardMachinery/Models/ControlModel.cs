using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;

namespace ClipboardMachinery.Models {

    internal class ControlModel : PropertyChangedBase {

        public Geometry Icon { get; }

        public SolidColorBrush Color =>
            Application.Current.FindResource("PanelControlBrush") as SolidColorBrush;

        public ControlModel(string iconName) {
            Icon = Application.Current.FindResource(iconName) as Geometry;
        }

    }

}
