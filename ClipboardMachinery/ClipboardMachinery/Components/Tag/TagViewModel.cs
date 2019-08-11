using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Plumbing.Factories;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using ClipboardMachinery.DialogOverlays.TagEditor;

namespace ClipboardMachinery.Components.Tag {

    public class TagViewModel : Screen {

        #region Properties

        public TagModel Model {
            get => model;
            set {
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
            }
        }

        public bool CanEdit {
            get => canEdit;
            private set {
                if (canEdit == value) {
                    return;
                }

                canEdit = value;
                NotifyOfPropertyChange();
            }
        }

        public bool HasDescription
            => !string.IsNullOrWhiteSpace(Model.Description);

        public SolidColorBrush BackgroundColor
            => model.Color.HasValue
                ? new SolidColorBrush(Color.FromArgb(40, model.Color.Value.R, model.Color.Value.G, model.Color.Value.B))
                : Brushes.Transparent;

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IDialogOverlayFactory dialogOverlayFactory;

        private TagModel model;
        private bool canEdit = true;

        #endregion

        public TagViewModel(IEventAggregator eventAggregator, IDialogOverlayFactory dialogOverlayFactory) {
            this.eventAggregator = eventAggregator;
            this.dialogOverlayFactory = dialogOverlayFactory;
        }

        #region Handlers

        private void OnTagEditorDeactivated(object sender, DeactivationEventArgs e) {
            TagEditorViewModel tagEditor = (TagEditorViewModel)sender;
            tagEditor.Deactivated -= OnTagEditorDeactivated;
            dialogOverlayFactory.Release(tagEditor);
            CanEdit = true;
        }

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
            TagEditorViewModel tagEditor = dialogOverlayFactory.CreateTagEditor(Model);
            tagEditor.Deactivated += OnTagEditorDeactivated;
            CanEdit = false;
            eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Show(tagEditor));
        }

        #endregion

    }

}
