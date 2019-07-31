using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;

namespace ClipboardMachinery.Components.TagType {

    public class TagTypeViewModel : Screen {

        #region Properties

        public TagTypeModel Model {
            get => model;
            private set {
                if (model == value) {
                    return;
                }

                model = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsFocused {
            get => isFocused;
            set {
                if (isFocused == value) {
                    return;
                }

                isFocused = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => SelectionColor);
            }
        }

        public SolidColorBrush SelectionColor
            => Application.Current.FindResource(IsFocused ? "ElementSelectBrush" : "PanelControlBrush") as SolidColorBrush;

        #endregion

        #region Fields

        private TagTypeModel model;
        private bool isFocused;

        #endregion

        public TagTypeViewModel(TagTypeModel model) {
            Model = model;
        }

        #region Actions

        public void Focus() {
            IsFocused = true;
        }

        public void Unfocus() {
            IsFocused = false;
        }

        #endregion

    }

}
