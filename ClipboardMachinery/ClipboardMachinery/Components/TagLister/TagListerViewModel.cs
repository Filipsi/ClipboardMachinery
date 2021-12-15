using System.Collections;
using Caliburn.Micro;
using Castle.Core;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Plumbing.Factories;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using ClipboardMachinery.Common.Helpers;

namespace ClipboardMachinery.Components.TagLister {

    public class TagListerViewModel : Conductor<TagViewModel>.Collection.AllActive, IHandle<TagEvent> {

        #region Properties

        [DoNotWire]
        public ClipModel Clip {
            get => clip;
            set {
                if (clip == value) {
                    return;
                }

                if (clip != null) {
                    clip.Tags.CollectionChanged -= OnModelTagCollectionChanged;
                    OnModelTagCollectionChanged(clip.Tags, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }

                clip = value;

                if (value != null) {
                    value.Tags.CollectionChanged += OnModelTagCollectionChanged;
                    OnModelTagCollectionChanged(value.Tags, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value.Tags.ToArray()));
                }

                NotifyOfPropertyChange();
            }
        }

        public bool CanOpenTagOverview
            => dialogOverlayManager != null;

        #endregion

        #region Fields

        private readonly IClipFactory clipFactory;
        private readonly IDialogOverlayManager dialogOverlayManager;

        private ClipModel clip;

        #endregion

        public TagListerViewModel(IClipFactory clipFactory, IDialogOverlayManager dialogOverlayManager) {
            this.clipFactory = clipFactory;
            this.dialogOverlayManager = dialogOverlayManager;
        }

        public TagListerViewModel(ClipModel clip, IClipFactory clipFactory, IDialogOverlayManager dialogOverlayManager) : this(clipFactory, dialogOverlayManager)  {
            Clip = clip;
        }

        public TagListerViewModel(ClipModel clip, IClipFactory clipFactory) {
            this.clipFactory = clipFactory;
            Clip = clip;
        }

        #region Handlers

        private async void OnModelTagCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (TagModel tagModel in e.NewItems) {
                        TagViewModel vm = clipFactory.CreateTag(tagModel);
                        await ActivateItemAsync(vm, CancellationToken.None);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (TagModel tagModel in e.OldItems) {
                        foreach (TagViewModel oldVm in Items.Where(vm => vm.Model == tagModel).ToArray()) {
                            await DeactivateItemAsync(oldVm, true, CancellationToken.None);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    // Remove view models that does not have matching tag model in the clip
                    foreach (TagViewModel oldVm in Items.Where(vm => Clip.Tags.All(tag => tag.Id != vm.Model.Id)).ToArray()) {
                        await DeactivateItemAsync(oldVm, true, CancellationToken.None);
                    }

                    // Create view models for tags that are in the clip tag list
                    foreach (TagModel tagModel in Clip.Tags.Where(tag => Items.All(vm => vm.Model.Id != tag.Id)).ToArray()) {
                        TagViewModel vm = clipFactory.CreateTag(tagModel);
                        await ActivateItemAsync(vm, CancellationToken.None);
                    }
                    break;
            }

            // TODO, FIXME: Optimize this
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Reset:
                    ArrayList.Adapter((IList)Items).Sort(new ComparisonComparer<TagViewModel>(
                        (a, b) => (-a.Model.Priority).CompareTo(-b.Model.Priority)
                    ));
                    break;
            }
        }

        public Task HandleAsync(TagEvent message, CancellationToken cancellationToken) {
            switch (message.EventType) {
                case TagEvent.TagEventType.TagAdded:
                    if (message.RelatedClipId == Clip.Id) {
                        Clip.Tags.Add((TagModel)message.Argument);
                    }
                    break;

                case TagEvent.TagEventType.TagRemoved:
                    foreach (TagViewModel tagToRemove in Items.Where(vm => vm.Model.Id == message.TagId).ToArray()) {
                        Clip.Tags.Remove(tagToRemove.Model);
                    }
                    break;

                case TagEvent.TagEventType.TypeRemoved:
                    foreach (TagViewModel tagToRemove in Items.Where(vm => vm.Model.TypeName == message.TagTypeName).ToArray()) {
                        Clip.Tags.Remove(tagToRemove.Model);
                    }
                    break;

                case TagEvent.TagEventType.TypeColorChanged:
                    foreach (TagViewModel vm in Items.Where(vm => vm.Model.TypeName == message.TagTypeName)) {
                        vm.Model.Color = (Color)message.Argument;
                    }
                    break;

                case TagEvent.TagEventType.TypePriorityChanged:
                    foreach (TagViewModel vm in Items.Where(vm => vm.Model.TypeName == message.TagTypeName)) {
                        vm.Model.Priority = (byte)message.Argument;
                    }

                    ArrayList.Adapter((IList)Items).Sort(new ComparisonComparer<TagViewModel>(
                        (a, b) => (-a.Model.Priority).CompareTo(-b.Model.Priority)
                    ));
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
                Clip = null;
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion

        #region Actions

        public async Task OpenTagOverview() {
            if (dialogOverlayManager == null) {
                return;
            }

            await dialogOverlayManager.OpenDialog(
                createFn: () => dialogOverlayManager.Factory.CreateTagOverview(Clip),
                releaseFn: dialog => dialogOverlayManager.Factory.Release(dialog)
            );
        }

        #endregion

    }

}
