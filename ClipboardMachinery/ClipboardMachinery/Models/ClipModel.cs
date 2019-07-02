using Caliburn.Micro;
using System;

namespace ClipboardMachinery.Models
{

    public class ClipModel : PropertyChangedBase {

        #region Properties

        public string RawContent {
            get => rawContent;
            set {
                if (value == rawContent)
                    return;

                rawContent = value;
                NotifyOfPropertyChange();
            }
        }

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
