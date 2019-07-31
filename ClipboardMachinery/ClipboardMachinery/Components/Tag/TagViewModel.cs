using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Plumbing.Factories;
using ClipboardMachinery.Popup.TagEditor;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

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

        public SolidColorBrush BackgroundColor
            => model.Color.HasValue
                ? new SolidColorBrush(Color.FromArgb(40, model.Color.Value.R, model.Color.Value.G, model.Color.Value.B))
                : Brushes.Transparent;

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IPopupFactory popupFactory;

        private TagModel model;
        private bool canEdit = true;

        #endregion

        public TagViewModel(IEventAggregator eventAggregator, IPopupFactory popupFactory) {
            this.eventAggregator = eventAggregator;
            this.popupFactory = popupFactory;
        }

        #region Actions

        public void Edit() {
            TagEditorViewModel tagEditor = popupFactory.CreateTagEditor(model);
            tagEditor.Deactivated += OnTagEditorDeactivated;
            CanEdit = false;
            eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Show(tagEditor));
        }

        #endregion

        #region Handlers

        private void OnTagEditorDeactivated(object sender, DeactivationEventArgs e) {
            TagEditorViewModel tagEditor = (TagEditorViewModel)sender;
            tagEditor.Deactivated -= OnTagEditorDeactivated;
            popupFactory.Release(tagEditor);
            CanEdit = true;
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(TagModel.Color)) {
                NotifyOfPropertyChange(() => BackgroundColor);
            }
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            if (close) {
                Model = null;
            }

            return Task.CompletedTask;
        }

        #endregion

    }

}
