using System;
using System.Windows;
using System.Windows.Media;

namespace ClipboardMachinery.Models {

    internal class ActionButtonModel : BaseControlModel {

        public bool IsFocused {
            get => _isFocused;
            private set {
                if (value == _isFocused) return;
                _isFocused = value;
                NotifyOfPropertyChange(() => IsFocused);
                NotifyOfPropertyChange(() => Color);
            }
        }

        public new SolidColorBrush Color =>
            Application.Current.FindResource(IsFocused ? "PanelSelectedBrush" : "PanelControlBrush") as SolidColorBrush;

        private readonly Action _clickAction;
        private bool _isFocused;

        public ActionButtonModel(string iconName, Action clickAction) : base(iconName) {
            _clickAction = clickAction;
        }

        public void InvokeClickAction() {
            _clickAction.Invoke();
        }

        public void Focus() {
            IsFocused = true;
        }

        public void Unfocus() {
            IsFocused = false;
        }

    }

}
