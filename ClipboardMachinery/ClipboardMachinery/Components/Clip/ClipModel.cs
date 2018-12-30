using Caliburn.Micro;
using ClipboardMachinery.Components.Tag;
using Newtonsoft.Json;
using System;

namespace ClipboardMachinery.Components.Clip {

    [JsonObject(MemberSerialization.OptIn)]
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

        [JsonProperty("content")]
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

        [JsonProperty("timestamp")]
        public DateTime Created {
            get => created;
            set {
                if (created == value) {
                    return;
                }

                created = value;
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
        private DateTime created;

        #endregion

        public ClipModel() {
            Tags = new BindableCollection<TagModel>();
        }

    }

}
