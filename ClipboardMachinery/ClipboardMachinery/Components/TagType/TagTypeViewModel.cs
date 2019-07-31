using Caliburn.Micro;

namespace ClipboardMachinery.Components.TagType {

    public class TagTypeViewModel : Screen {

        #region Properties

        public TagTypeModel Model {
            get => model;
            private set {
                if (model == value) {
                    return;
                }

                model = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Fields

        private TagTypeModel model;

        #endregion

        public TagTypeViewModel(TagTypeModel model) {
            Model = model;
        }

    }

}
