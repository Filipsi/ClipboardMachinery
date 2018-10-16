using System;
using System.Windows;
using System.Windows.Media;

namespace ClipboardMachinery.Common.Models {

    public class ActionButtonModel : ControlModel {

        #region Properties

        public bool IsFocused {
            get => isFocused;
            private set {
                if (isFocused == value) {
                    return;
                }

                isFocused = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => Color);
            }
        }

        public SolidColorBrush SelectedColor {
            get => selectedColor;
            set {
                if (selectedColor == value) {
                    return;
                }

                selectedColor = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => Color);
            }
        }

        public new SolidColorBrush Color
            => IsFocused ? SelectedColor : base.Color;

        #endregion

        #region Fields

        private readonly Action clickAction;
        private bool isFocused;
        private SolidColorBrush selectedColor;

        #endregion

        public ActionButtonModel(string iconName, Action clickAction, string selectColor = null) : base(iconName) {
            this.clickAction = clickAction;
            SelectedColor = (SolidColorBrush) Application.Current.FindResource(
                string.IsNullOrEmpty(selectColor) ? "PanelSelectedBrush" : selectColor
            );
        }

        #region Logic

        public void InvokeClickAction() {
            clickAction.Invoke();
        }

        public void Focus() {
            IsFocused = true;
        }

        public void Unfocus() {
            IsFocused = false;
        }

        #endregion

    }

}
