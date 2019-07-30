using System.Threading.Tasks;
using ClipboardMachinery.Components.Buttons.ActionButton;
using System.Windows;
using System.Windows.Media;

namespace ClipboardMachinery.Components.Buttons.SelectableButton {

    public class SelectableButtonViewModel : ActionButtonViewModel {

        #region Properties

        public SolidColorBrush SelectionColor {
            get => selectionColor;
            set {
                if (Equals(selectionColor, value)) {
                    return;
                }

                selectionColor = value;
                NotifyOfPropertyChange();

                if (IsSelected) {
                    NotifyOfPropertyChange(() => Color);
                }
            }
        }

        public bool IsSelected {
            get => isSelected;
            set {
                if (isSelected == value) {
                    return;
                }

                isSelected = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => Color);
            }
        }

        public new SolidColorBrush Color {
            get {
                if (!IsEnabled) {
                    return DisabledColor;
                }

                return IsSelected
                    ? SelectionColor
                    : (IsFocused ? HoverColor : DefaultColor);
            }
        }

        #endregion

        #region Fields

        protected static readonly SolidColorBrush panelSelectionColor = Application.Current.FindResource("PanelSelectedBrush") as SolidColorBrush;

        protected SolidColorBrush selectionColor = panelSelectionColor;
        protected bool isSelected;

        #endregion

        #region Actions

        public override async Task Click() {
            if (IsEnabled && !IsSelected) {
                IsEnabled = false;
                IsSelected = true;
                await clickHandler.Invoke(this);
                IsEnabled = true;
            }
        }

        #endregion

    }

}
