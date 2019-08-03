using Caliburn.Micro;
using System.Windows.Media;
using ClipboardMachinery.Core.DataStorage;

namespace ClipboardMachinery.Components.Tag {

    public class TagModel : PropertyChangedBase {

        #region Properties

        public int Id {
            get => id;
            set {
                if (id == value) {
                    return;
                }

                id = value;
                NotifyOfPropertyChange();
            }
        }

        public string TypeName {
            get => typeName;
            set {
                if (typeName == value) {
                    return;
                }

                typeName = value;
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

        public string Description {
            get => description;
            set {
                if (description == value) {
                    return;
                }

                description = value;
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

        private static readonly Color defaultColor = System.Windows.Media.Color.FromArgb(
            SystemTagTypes.DefaultColor.A,
            SystemTagTypes.DefaultColor.R,
            SystemTagTypes.DefaultColor.G,
            SystemTagTypes.DefaultColor.B
        );

        private int id;
        private string typeName;
        private object val;
        private Color? color = defaultColor;
        private string description;

        #endregion

    }

}
