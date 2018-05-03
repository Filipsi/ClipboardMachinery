using System;
using System.Windows;
using System.Windows.Media;

namespace ClipboardMachinery.Models {

    internal class PageNavigatorModel : ControlModel {

        public string Name { get; }

        public Type ViewModelType { get; }

        public new SolidColorBrush Color =>
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

        public PageNavigatorModel(string name, string iconName, Type viewModelType) : base(iconName) {
            Name = name;
            ViewModelType = viewModelType;
        }

    }

}
