using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Core.Data;
using ClipboardMachinery.Plumbing.Factories;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static ClipboardMachinery.Common.Events.ClipEvent;

namespace ClipboardMachinery.Pages {

    public abstract class ClipHolder : Conductor<ClipViewModel>.Collection.AllActive, IHandle<ClipEvent> {

        #region Properties

        public bool WatermarkIsVisible
            => Items.Count == 0;

        #endregion

        #region Fields

        protected readonly IDataRepository dataRepository;
        protected readonly IClipViewModelFactory clipVmFactory;

        protected bool allowAddingClipsFromKeyboard = true;

        #endregion

        protected ClipHolder(IDataRepository dataRepository, IClipViewModelFactory clipVmFactory) {
            this.dataRepository = dataRepository;
            this.clipVmFactory = clipVmFactory;

            Items.CollectionChanged += OnClipboardItemsCollectionChanged;
        }

        #region Handlers

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            if (close) {
                Items.CollectionChanged -= OnClipboardItemsCollectionChanged;
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        private void OnClipboardItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    NotifyOfPropertyChange(() => WatermarkIsVisible);
                    break;
            }
        }

        public async Task HandleAsync(ClipEvent message, CancellationToken cancellationToken) {
            switch (message.EventType) {
                case ClipEventType.Created:
                    if (!allowAddingClipsFromKeyboard) {
                        return;
                    }

                    ClipViewModel createdClip = clipVmFactory.Create(message.Source);
                    Items.Insert(0, createdClip);
                    await ActivateItemAsync(createdClip, cancellationToken);
                    OnKeyboardClipAdded(createdClip);
                    break;

                case ClipEventType.Remove:
                    ClipViewModel clipToRemove = Items.FirstOrDefault(vm => vm.Model.Id == message.Source.Id);
                    if (clipToRemove != null) {
                        await Task.Run(() => dataRepository.DeleteClip(clipToRemove.Model.Id), cancellationToken);
                        await clipToRemove.TryCloseAsync();
                        clipVmFactory.Release(clipToRemove);
                    }
                    break;

                case ClipEventType.ToggleFavorite:
                    ClipViewModel clipVm = Items.FirstOrDefault(vm => vm.Model.Id == message.Source.Id);
                    if (clipVm != null) {
                        ClipModel clip = clipVm.Model;
                        TagModel favoriteTag = clip.Tags.FirstOrDefault(
                            tag => tag.Name == "category" && tag.Value.ToString() == "favorite"
                        );

                        if (favoriteTag == null) {
                            await Task.Run(async () => {
                                TagModel newTag = await dataRepository.CreateTag<TagModel>(
                                    clipId: clip.Id,
                                    name: "category",
                                    value: "favorite"
                                );

                                await Application.Current.Dispatcher.InvokeAsync(() => clip.Tags.Add(newTag));
                            }, cancellationToken);
                        } else {
                            await Task.Run(() => dataRepository.DeleteTag(favoriteTag.Id), cancellationToken);
                            clip.Tags.Remove(favoriteTag);
                        }
                    }
                    break;
            }
        }

        protected virtual void OnKeyboardClipAdded(ClipViewModel newClip) {
        }

        #endregion

    }

}
