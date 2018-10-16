using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;

namespace ClipboardMachinery.Common.Models {

    public class ControlModel : PropertyChangedBase {

        public Geometry Icon {
            get;
        }

        public SolidColorBrush Color {
            get;
        }

        public ControlModel(string iconName) {
            Icon = Application.Current.FindResource(iconName) as Geometry;
            Color = Application.Current.FindResource("PanelControlBrush") as SolidColorBrush;
        }

    }

}
