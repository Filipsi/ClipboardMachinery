using Caliburn.Micro;
using System;
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
                if (icon == value) {
                    return;
                }

                icon = value;
                NotifyOfPropertyChange();
            }
        }

        public SolidColorBrush DefaultColor {
            get => defaultColor;
            set {
                if (defaultColor == value) {
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
                if (hoverColor == value) {
                    return;
                }

                hoverColor = value;
                NotifyOfPropertyChange();

                if (IsFocused) {
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

        public Action<ActionButtonViewModel> ClickAction {
            get => clickAction;
            set {
                if (clickAction == value) {
                    return;
                }

                clickAction = value;
                NotifyOfPropertyChange();
            }
        }

        public SolidColorBrush Color {
            get {
                if (!IsEnabled) {
                    return disabledColor;
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

        protected static readonly SolidColorBrush panelDefaultColor = Application.Current.FindResource("PanelControlBrush") as SolidColorBrush;
        protected static readonly SolidColorBrush panelHoverColor = Application.Current.FindResource("PanelHoverBrush") as SolidColorBrush;
        protected static readonly SolidColorBrush disabledColor = Brushes.DimGray;

        protected Action<ActionButtonViewModel> clickAction;
        protected SolidColorBrush defaultColor = panelDefaultColor;
        protected SolidColorBrush hoverColor = panelHoverColor;
        protected bool isEnabled = true;
        protected bool isFocused;
        protected Geometry icon;
        protected object model;
        protected string toolTip;

        #endregion

        #region Actions

        public virtual void Click() {
            if (IsEnabled) {
                clickAction.Invoke(this);
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
