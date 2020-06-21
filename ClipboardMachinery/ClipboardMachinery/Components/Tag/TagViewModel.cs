using Caliburn.Micro;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using ClipboardMachinery.Components.DialogOverlay;
using Castle.Core;

namespace ClipboardMachinery.Components.Tag {

    public class TagViewModel : Screen {

        #region Properties

        [DoNotWire]
        public TagModel Model {
            get => model;
            private set {
                if (model == value) {
                    return;
                }

                if (model != null) {
                    model.PropertyChanged -= OnModelPropertyChanged;
                }

                if (value != null) {
                    value.PropertyChanged += OnModelPropertyChanged;
                }

                model = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => HasDescription);
            }
        }

        public bool HasDescription
            => !string.IsNullOrWhiteSpace(Model?.Description);

        public Color BackgroundColor
            => Model?.Color.HasValue == true ? Model.Color.Value : Colors.Transparent;

        #endregion

        #region Fields

        private readonly IDialogOverlayManager dialogOverlayManager;

        private TagModel model;

        #endregion

        public TagViewModel(TagModel tagModel, IDialogOverlayManager dialogOverlayManager) {
            this.dialogOverlayManager = dialogOverlayManager;
            Model = tagModel;
        }

        #region Handlers

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(TagModel.Color):
                    NotifyOfPropertyChange(() => BackgroundColor);
                    break;

                case nameof(TagModel.Description):
                    NotifyOfPropertyChange(() => HasDescription);
                    break;
            }
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            if (close) {
                Model = null;
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Actions

        public void Edit() {
            dialogOverlayManager.OpenDialog(
                () => dialogOverlayManager.Factory.CreateTagEditor(Model),
                tagEditor => dialogOverlayManager.Factory.Release(tagEditor)
            );
        }

        #endregion

    }

}
