using Caliburn.Micro;
using System.Windows;
using System.Windows.Media;
using Action = System.Action;

namespace ClipboardMachinery.Components.ActionButton {

    public class ActionButtonViewModel : Screen {

        #region Properties

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

        public SolidColorBrush Color
            => IsFocused ? HoverColor : defaultColor;

        public Action ClickAction {
            get => clickAction;
            set {
                if (clickAction == value) {
                    return;
                }

                clickAction = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => Color);
            }
        }

        #endregion

        #region Fields

        private Action clickAction;
        private bool isFocused;
        private SolidColorBrush hoverColor;
        private SolidColorBrush defaultColor;
        private Geometry icon;

        #endregion

        public ActionButtonViewModel() {
            defaultColor = Application.Current.FindResource("PanelControlBrush") as SolidColorBrush;
            hoverColor = (SolidColorBrush)Application.Current.FindResource("PanelSelectedBrush");
        }

        #region Logic

        public void Click()
            => clickAction.Invoke();

        public void Focus()
            => IsFocused = true;

        public void Unfocus()
            => IsFocused = false;

        #endregion

    }

}
