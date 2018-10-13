using Caliburn.Micro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ClipboardMachinery.Components.Clip {

    [JsonObject(MemberSerialization.OptIn)]
    public class ClipModel : PropertyChangedBase {

        #region Properties

        [JsonProperty("content")]
        public string RawContent {
            get => rawContent;
            set {
                if (value == rawContent)
                    return;

                rawContent = value;
                NotifyOfPropertyChange();
            }
        }

        [JsonProperty("timestamp")]
        public DateTime Created {
            get => created;
            set {
                if (value == created)
                    return;

                created = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsFavorite {
            get => isFavorite;
            set {
                if (value == isFavorite)
                    return;

                isFavorite = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsFocused {
            get => isFocused;
            set {
                if (value == isFocused)
                    return;

                isFocused = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Fields

        private string rawContent;
        private DateTime created;
        private bool isFavorite;
        private bool isFocused;

        #endregion

    }

}
