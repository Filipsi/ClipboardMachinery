using System;
using System.Windows;
using System.Windows.Media;

namespace ClipboardMachinery.Models {

    public class PageNavigatorModel : ControlModel {

        #region Properties

        public string Name {
            get;
        }

        public Type ViewModelType {
            get;
        }

        public new SolidColorBrush Color =>
            Application.Current.FindResource(IsSelected ? "PanelSelectedBrush" : "PanelControlBrush") as SolidColorBrush;

        public bool IsSelected {
            get => isSelected;
            set {
                if (value == isSelected)
                    return;

                isSelected = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Color));
            }
        }


        #endregion

        #region Fields

        private bool isSelected;

        #endregion

        public PageNavigatorModel(string name, string iconName, Type viewModelType) : base(iconName) {
            Name = name;
            ViewModelType = viewModelType;
        }

    }

}
