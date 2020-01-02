using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using ClipboardMachinery.Components.Tag;

namespace ClipboardMachinery.Components.Clip {

    public class ClipModel : PropertyChangedBase {

        #region Properties

        public int Id {
            get => id;
            private set {
                if (id == value) {
                    return;
                }

                id = value;
                NotifyOfPropertyChange();
            }
        }

        public string Content {
            get => content;
            private set {
                if (content == value) {
                    return;
                }

                content = value;
                NotifyOfPropertyChange();
            }
        }

        public string Presenter {
            get => presenter;
            set {
                if (presenter == value) {
                    return;
                }

                presenter = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<TagModel> Tags {
            get;
        }

        #endregion

        #region Fields

        private int id;
        private string content;
        private string presenter;

        #endregion

        public ClipModel(int id, string content, IEnumerable<TagModel> tags) {
            Id = id;
            Content = content;
            Tags = new BindableCollection<TagModel>(tags ?? Enumerable.Empty<TagModel>());
        }

    }

}
