using Caliburn.Micro;
using ClipboardMachinery.Components.Tag;
using System;

namespace ClipboardMachinery.Components.Clip {

    public class ClipModel : PropertyChangedBase {

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

        public string Content {
            get => rawContent;
            set {
                if (rawContent == value) {
                    return;
                }

                rawContent = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<TagModel> Tags {
            set; get;
        }

        #endregion

        #region Fields

        private int id;
        private string rawContent;

        #endregion

        public ClipModel() {
            Tags = new BindableCollection<TagModel>();
        }

    }

}
