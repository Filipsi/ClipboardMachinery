using Caliburn.Micro;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ClipboardMachinery.Components.Buttons.ActionButton {

    public class ActionButtonViewModel : Screen {

        #region Properties

        public object Model {
            get => model;
            set {
                if (model == value) {
                    return;
                }

                model = value;
                NotifyOfPropertyChange();
            }
        }

        public string ToolTip {
            get => toolTip;
            set {
                if (toolTip == value) {
                    return;
                }

                toolTip = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => HasToolTip);
            }
        }

        public Geometry Icon {
            get => icon;
            set {
                if (Equals(icon, value)) {
                    return;
                }

                icon = value;
                NotifyOfPropertyChange();
            }
        }

        public SolidColorBrush DefaultColor {
            get => defaultColor;
            set {
                if (Equals(defaultColor, value)) {
                    return;
                }

                defaultColor = value;
                NotifyOfPropertyChange();

                if (!IsFocused) {
                    NotifyOfPropertyChange(() => Color);
                }
            }
        }

        public SolidColorBrush HoverColor {
            get => hoverColor;
            set {
                if (Equals(hoverColor, value)) {
                    return;
                }

                hoverColor = value;
                NotifyOfPropertyChange();

                if (IsFocused) {
                    NotifyOfPropertyChange(() => Color);
                }
            }
        }

        public SolidColorBrush DisabledColor {
            get => disabledColor;
            set {
                if (Equals(disabledColor, value)) {
                    return;
                }

                disabledColor = value;
                NotifyOfPropertyChange();

                if (!IsEnabled) {
                    NotifyOfPropertyChange(() => Color);
                }
            }
        }

        public bool IsEnabled {
            get => isEnabled;
            set {
                if (isEnabled == value) {
                    return;
                }

                isEnabled = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => Color);
            }
        }

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

        public Func<ActionButtonViewModel, Task> ClickAction {
            get => clickHandler;
            set {
                if (clickHandler == value) {
                    return;
                }

                clickHandler = value;
                NotifyOfPropertyChange();
            }
        }

        public SolidColorBrush Color {
            get {
                if (!IsEnabled) {
                    return DisabledColor;
                }

                return IsFocused
                    ? HoverColor
                    : DefaultColor;
            }
        }

        public bool HasToolTip
            => !string.IsNullOrEmpty(ToolTip);

        #endregion

        #region Fields

        protected static readonly SolidColorBrush defaultForegroundColor = Application.Current.FindResource("PanelControlBrush") as SolidColorBrush;
        protected static readonly SolidColorBrush defaultHoverColor = Application.Current.FindResource("PanelHoverBrush") as SolidColorBrush;
        protected static readonly SolidColorBrush defaultDisabledColor = Brushes.DimGray;

        private SolidColorBrush defaultColor = defaultForegroundColor;
        private SolidColorBrush hoverColor = defaultHoverColor;
        private SolidColorBrush disabledColor = defaultDisabledColor;
        protected Func<ActionButtonViewModel, Task> clickHandler;
        protected bool isEnabled = true;
        protected bool isFocused;
        protected Geometry icon;
        protected object model;
        protected string toolTip;

        #endregion

        #region Actions

        public virtual async Task Click() {
            if (IsEnabled) {
                IsEnabled = false;
                await clickHandler.Invoke(this);
                IsEnabled = true;
            }
        }

        public virtual void Focus() {
            if (IsEnabled) {
                IsFocused = true;
            }
        }

        public virtual void Unfocus()
            => IsFocused = false;

        #endregion

    }

}
