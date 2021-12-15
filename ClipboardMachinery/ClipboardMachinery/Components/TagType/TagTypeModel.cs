using System;
using System.Windows.Media;
using Caliburn.Micro;

namespace ClipboardMachinery.Components.TagType {

    public class TagTypeModel : PropertyChangedBase {

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

        public Type Kind {
            get => kind;
            set {
                if (kind == value) {
                    return;
                }

                kind = value;
                NotifyOfPropertyChange();
            }
        }

        public byte Priority {
            get => priority;
            set {
                if (priority == value) {
                    return;
                }

                priority = value;
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

        public Color Color {
            get => color;
            set {
                if (color == value) {
                    return;
                }

                color = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Fields

        private string name;
        private Type kind;
        private byte priority;
        private string description;
        private Color color;

        #endregion

    }

}
