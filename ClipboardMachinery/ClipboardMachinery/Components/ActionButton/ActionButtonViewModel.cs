using Caliburn.Micro;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ClipboardMachinery.Components.ActionButton {

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

        public SolidColorBrush SelectionColor {
            get => selectionColor;
            set {
                if (selectionColor == value) {
                    return;
                }

                selectionColor = value;
                NotifyOfPropertyChange();

                if (IsSelected) {
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

        public bool CanBeSelected {
            get => canBeSelected;
            set {
                if (canBeSelected == value) {
                    return;
                }

                canBeSelected = value;
                NotifyOfPropertyChange();

                if (!canBeSelected) {
                    IsSelected = false;
                }
            }
        }

        public bool IsSelected {
            get => isSelected;
            set {
                if(!CanBeSelected) {
                    return;
                }

                if (isSelected == value) {
                    return;
                }

                isSelected = value;
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

                return IsSelected
                    ? SelectionColor
                    : (IsFocused ? HoverColor : DefaultColor);
            }
        }

        public bool HasToolTip
            => !string.IsNullOrEmpty(ToolTip);

        #endregion

        #region Fields

        private static readonly SolidColorBrush panelDefaultColor = Application.Current.FindResource("PanelControlBrush") as SolidColorBrush;
        private static readonly SolidColorBrush panelHoverColor = Application.Current.FindResource("PanelHoverBrush") as SolidColorBrush;
        private static readonly SolidColorBrush panelSelectionColor = Application.Current.FindResource("PanelSelectedBrush") as SolidColorBrush;
        private static readonly SolidColorBrush disabledColor = Brushes.DimGray;

        private Action<ActionButtonViewModel> clickAction;
        private SolidColorBrush defaultColor = panelDefaultColor;
        private SolidColorBrush hoverColor = panelHoverColor;
        private SolidColorBrush selectionColor = panelSelectionColor;
        private bool isEnabled = true;
        private bool isFocused;
        private bool isSelected;
        private bool canBeSelected;
        private Geometry icon;
        private object model;
        private string toolTip;

        #endregion

        #region Actions

        public void Click() {
            if (!IsEnabled) {
                return;
            }

            if (CanBeSelected) {
                if (!IsSelected) {
                    IsSelected = true;
                    clickAction.Invoke(this);
                }
            } else {
                clickAction.Invoke(this);
            }
        }

        public void Focus() {
            if (IsEnabled) {
                IsFocused = true;
            }
        }

        public void Unfocus()
            => IsFocused = false;

        #endregion

    }

}
