using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Components.Tag {

    public class TagModel : PropertyChangedBase {

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

        private string name;
        private object val;

    }

}
