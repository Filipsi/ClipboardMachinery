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

        public SolidColorBrush Color
            => IsSelected
                ? SelectionColor
                : (IsFocused ? HoverColor : DefaultColor);

        public bool HasToolTip
            => !string.IsNullOrEmpty(ToolTip);

        #endregion

        #region Fields

        private Action<ActionButtonViewModel> clickAction;
        private bool isFocused;
        private bool isSelected;
        private bool canBeSelected;
        private SolidColorBrush hoverColor;
        private SolidColorBrush defaultColor;
        private SolidColorBrush selectionColor;
        private Geometry icon;
        private object model;
        private string toolTip;

        #endregion

        public ActionButtonViewModel() {
            defaultColor = Application.Current.FindResource("PanelControlBrush") as SolidColorBrush;
            hoverColor = Application.Current.FindResource("PanelHoverBrush") as SolidColorBrush;
            selectionColor = Application.Current.FindResource("PanelSelectedBrush") as SolidColorBrush;
        }

        #region Logic

        public void Click() {
            if (CanBeSelected) {
                if (!IsSelected) {
                    IsSelected = true;
                    clickAction.Invoke(this);
                }
            } else {
                clickAction.Invoke(this);
            }
        }

        public void Focus()
            => IsFocused = true;

        public void Unfocus()
            => IsFocused = false;

        #endregion

    }

}
