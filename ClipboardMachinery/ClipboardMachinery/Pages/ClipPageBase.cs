using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Core.Repository;
using ClipboardMachinery.Plumbing.Factories;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static ClipboardMachinery.Common.Events.ClipEvent;

namespace ClipboardMachinery.Pages {

    public abstract class ClipPageBase : Conductor<ClipViewModel>.Collection.AllActive, IHandle<ClipEvent> {

        #region Properties

        public bool WatermarkIsVisible
            => Items.Count == 0;

        #endregion

        #region Fields

        protected readonly IDataRepository dataRepository;
        protected readonly IClipViewModelFactory clipVmFactory;

        protected bool AllowAddingClipsFromKeyboard = true;

        #endregion

        public ClipPageBase(IDataRepository dataRepository, IClipViewModelFactory clipVmFactory) {
            this.dataRepository = dataRepository;
            this.clipVmFactory = clipVmFactory;

            Items.CollectionChanged += OnClipboardItemsCollectionChanged;
        }

        #region Handlers

        protected override void OnDeactivate(bool close) {
            base.OnDeactivate(close);

            if (close) {
                Items.CollectionChanged -= OnClipboardItemsCollectionChanged;
            }
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
                    if (!AllowAddingClipsFromKeyboard) {
                        return;
                    }

                    ClipViewModel createdClip = clipVmFactory.Create(message.Source);
                    Items.Insert(0, createdClip);
                    OnKeyboardClipAdded(createdClip);
                    break;

                case ClipEventType.Remove:
                    ClipViewModel clipToRemove = Items.FirstOrDefault(vm => vm.Model.Id == message.Source.Id);
                    if (clipToRemove != null) {
                        Items.Remove(clipToRemove);
                        await dataRepository.DeleteClip(clipToRemove.Model.Id);
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
                            TagModel newTag = await dataRepository.CreateTag<TagModel>(
                                clipId: clip.Id,
                                name: "category",
                                value: "favorite"
                            );

                            clip.Tags.Add(newTag);
                        } else {
                            await dataRepository.DeleteTag(favoriteTag.Id);
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
