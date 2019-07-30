using System.Threading.Tasks;
using ClipboardMachinery.Components.Buttons.ActionButton;
using System.Windows.Media;

namespace ClipboardMachinery.Components.Buttons.ToggleButton {

    public class ToggleButtonViewModel : ActionButtonViewModel {

        #region Properties

        public new Geometry Icon {
            get => icon;
            set {
                if (Equals(icon, value)) {
                    return;
                }

                icon = value;
                NotifyOfPropertyChange();

                if (ToggledIcon == null) {
                    ToggledIcon = Icon;
                }

                if (ToggleColor == null) {
                    ToggleColor = DefaultColor;
                }

                if (!IsToggled) {
                    NotifyOfPropertyChange(() => CurrentIcon);
                }
            }
        }

        public Geometry ToggledIcon {
            get => toggledIcon;
            set {
                if (Equals(toggledIcon, value)) {
                    return;
                }

                toggledIcon = value;
                NotifyOfPropertyChange();

                if (IsToggled) {
                    NotifyOfPropertyChange(() => CurrentIcon);
                }
            }
        }

        public SolidColorBrush ToggleColor {
            get => toggleColor;
            set {
                if (Equals(toggleColor, value)) {
                    return;
                }

                toggleColor = value;
                NotifyOfPropertyChange();

                if (IsToggled) {
                    NotifyOfPropertyChange(() => Color);
                }
            }
        }

        public bool IsToggled {
            get => isToggled;
            set {
                if (isToggled == value) {
                    return;
                }

                isToggled = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => Color);
                NotifyOfPropertyChange(() => CurrentIcon);
            }
        }

        public new SolidColorBrush Color {
            get {
                if (!IsEnabled) {
                    return DisabledColor;
                }

                return IsToggled
                    ? ToggleColor
                    : (IsFocused ? HoverColor : DefaultColor);
            }
        }

        public Geometry CurrentIcon
            => IsToggled ? ToggledIcon : Icon;

        #endregion

        #region Fields

        private Geometry toggledIcon;
        private SolidColorBrush toggleColor;
        private bool isToggled;

        #endregion

        #region Actions

        public override async Task Click() {
            if (IsEnabled) {
                IsEnabled = false;
                IsToggled = !IsToggled;
                await clickHandler.Invoke(this);
                IsEnabled = true;
            }
        }

        #endregion

    }

}
