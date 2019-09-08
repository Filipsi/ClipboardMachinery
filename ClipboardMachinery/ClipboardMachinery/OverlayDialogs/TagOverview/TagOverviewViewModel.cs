using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.OverlayDialogs.TagOverview {

    // TODO: Polish this along with clip model, this is basically just a copy from there... there should be a base class or something like that.
    public class TagOverviewViewModel : Conductor<TagViewModel>.Collection.AllActive, IOverlayDialog, IHandle<TagEvent> {

        #region Properties

        public BindableCollection<ActionButtonViewModel> DialogControls {
            get;
        }

        public bool IsOpen {
            get => isOpen;
            set {
                if (isOpen == value) {
                    return;
                }

                isOpen = value;
                NotifyOfPropertyChange();
            }
        }

        public bool AreControlsVisible {
            get => areControlsVisible;
            set {
                if (areControlsVisible == value) {
                    return;
                }

                areControlsVisible = value;
                NotifyOfPropertyChange();
            }
        }

        public ClipModel Model {
            get => model;
            private set {
                if (model == value) {
                    return;
                }

                if (model != null) {
                    model.Tags.CollectionChanged -= OnModelTagCollectionChanged;
                    OnModelTagCollectionChanged(model.Tags, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }

                model = value;

                if (value != null) {
                    value.Tags.CollectionChanged += OnModelTagCollectionChanged;
                    OnModelTagCollectionChanged(value.Tags, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value.Tags.ToArray()));
                }

                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Fields

        private readonly IClipFactory clipFactory;

        private bool isOpen;
        private bool areControlsVisible;
        private ClipModel model;

        #endregion

        public TagOverviewViewModel(ClipModel clip, IClipFactory clipFactory) {
            DialogControls = new BindableCollection<ActionButtonViewModel>();
            this.clipFactory = clipFactory;
            Model = clip;
        }

        #region Handlers

        private async void OnModelTagCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (TagModel tagModel in e.NewItems) {
                        TagViewModel vm = clipFactory.CreateTag();
                        vm.Model = tagModel;
                        await ActivateItemAsync(vm, CancellationToken.None);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (TagModel tagModel in e.OldItems) {
                        foreach (TagViewModel vm in Items.Where(vm => vm.Model == tagModel).ToArray()) {
                            await DeactivateItemAsync(vm, true, CancellationToken.None);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    if (Items?.Count > 0) {
                        foreach (TagViewModel vm in Items.ToArray()) {
                            await DeactivateItemAsync(vm, true, CancellationToken.None);
                        }
                    }
                    break;
            }
        }

        public Task HandleAsync(TagEvent message, CancellationToken cancellationToken) {
            switch (message.EventType) {
                case TagEvent.TagEventType.TagAdded:
                    if (message.RelatedClipId == Model.Id) {
                        Model.Tags.Add((TagModel)message.Argument);
                    }
                    break;

                case TagEvent.TagEventType.TagRemoved:
                    foreach (TagViewModel tagToRemove in Items.Where(vm => vm.Model.Id == message.TagId).ToArray()) {
                        Model.Tags.Remove(tagToRemove.Model);
                    }
                    break;

                case TagEvent.TagEventType.TypeRemoved:
                    foreach (TagViewModel tagToRemove in Items.Where(vm => vm.Model.TypeName == message.TagTypeName).ToArray()) {
                        Model.Tags.Remove(tagToRemove.Model);
                    }
                    break;

                case TagEvent.TagEventType.TypeColorChanged:
                    foreach (TagViewModel vm in Items.Where(vm => vm.Model.TypeName == message.TagTypeName)) {
                        vm.Model.Color = (Color)message.Argument;
                    }
                    break;

                case TagEvent.TagEventType.TypeDescriptionChanged:
                    foreach (TagViewModel vm in Items.Where(vm => vm.Model.TypeName == message.TagTypeName)) {
                        vm.Model.Description = (string)message.Argument;
                    }
                    break;

                case TagEvent.TagEventType.TagValueChanged:
                    TagViewModel tagWithChangedValue = Items.FirstOrDefault(tag => tag.Model.Id == message.TagId);
                    if (tagWithChangedValue != null) {
                        tagWithChangedValue.Model.Value = (string)message.Argument;
                    }
                    break;
            }

            return Task.CompletedTask;
        }

        public override Task DeactivateItemAsync(TagViewModel item, bool close, CancellationToken cancellationToken) {
            if (close) {
                clipFactory.Release(item);
            }

            return base.DeactivateItemAsync(item, close, cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            if (close) {
                Model = null;
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion

    }

}
