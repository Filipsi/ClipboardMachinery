using Caliburn.Micro;
using System.Windows.Media;

namespace ClipboardMachinery.Components.Tag {

    public class TagModel : PropertyChangedBase {

        #region Properties

        public string Name {
            get => name;
            set {
                if (name == value) {
                    return;
                }

                name = value;
                NotifyOfPropertyChange();
            }
        }

        public object Value {
            get => val;
            set {
                if (val == value) {
                    return;
                }

                val = value;
                NotifyOfPropertyChange();
            }
        }

        public Color? Color {
            get => color;
            set {
                if (color == value) {
                    return;
                }

                if (value == null) {
                    value = defaultColor;
                }

                color = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Fields

        private static readonly Color defaultColor = System.Windows.Media.Color.FromArgb(26, 46, 49, 49);

        private string name;
        private object val;
        private Color? color = defaultColor;

        #endregion

    }

}
