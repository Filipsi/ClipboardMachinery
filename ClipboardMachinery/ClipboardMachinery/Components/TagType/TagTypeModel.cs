using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                NotifyOfPropertyChange(() => DisplayKindLabel);
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

        public string DisplayKindLabel
            => $"with {KindNameMap[Kind]} value";

        #endregion

        #region Fields

        private static readonly IReadOnlyDictionary<Type, string> KindNameMap = new ReadOnlyDictionary<Type, string>(
            new Dictionary<Type, string> {
                { typeof(string), "text"    },
                { typeof(int),    "numeric" },
                { typeof(double), "decimal" },
            }
        );

        private string name;
        private Type kind;
        private string description;
        private Color color;

        #endregion

    }

}
