using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Action = System.Action;

namespace ClipboardMachinery.Models {

    public class SelectableActionModel : PropertyChangedBase {

        public string Name { get; }

        public Geometry Icon { get; }

        public Action Action { get; }

        public SolidColorBrush Color =>
            Application.Current.FindResource(IsSelected ? "PanelSelectedBrush" : "PanelControlBrush") as SolidColorBrush;

        public bool IsSelected {
            get => _isSelected;
            set {
                if (value == _isSelected) return;
                _isSelected = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Color));
            }
        }

        private bool _isSelected;

        public SelectableActionModel(string name, string iconName, Action action) {
            Name = name;
            Icon = Application.Current.FindResource(iconName) as Geometry;
            Action = action;
        }

    }

}
