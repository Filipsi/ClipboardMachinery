using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Plumbing.Factories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static ClipboardMachinery.Common.Events.ClipEvent;

namespace ClipboardMachinery.Pages {

    public abstract class ClipPageBase : LazyPageBase<ClipViewModel, ClipModel>, IHandle<ClipEvent> {

        #region Fields

        protected readonly IDataRepository dataRepository;
        protected readonly IViewModelFactory vmFactory;

        #endregion

        protected ClipPageBase(int batchSize, IDataRepository dataRepository, IViewModelFactory vmFactory)
            : base(dataRepository.CreateLazyClipProvider(batchSize)) {

            this.dataRepository = dataRepository;
            this.vmFactory = vmFactory;
        }

        #region Logic

        protected override ClipViewModel CreateItem(ClipModel model) {
            return vmFactory.CreateClip(model);
        }

        protected override void ReleaseItem(ClipViewModel instance) {
            vmFactory.Release(instance);
        }

        protected abstract bool IsAllowedAddClipsFromKeyboard(ClipEvent message);

        #endregion

        #region Handlers

        public async Task HandleAsync(ClipEvent message, CancellationToken cancellationToken) {
            switch (message.EventType) {
                case ClipEventType.Created:
                    if (!IsAllowedAddClipsFromKeyboard(message)) {
                        return;
                    }

                    ClipViewModel createdClip = vmFactory.CreateClip(message.Source);
                    Items.Insert(0, createdClip);
                    await ActivateItemAsync(createdClip, cancellationToken);
                    OnKeyboardClipAdded(createdClip);
                    break;

                case ClipEventType.Remove:
                    ClipViewModel clipToRemove = Items.FirstOrDefault(vm => vm.Model.Id == message.Source.Id);
                    if (clipToRemove != null) {
                        await Task.Run(() => dataRepository.DeleteClip(clipToRemove.Model.Id), cancellationToken);
                        await clipToRemove.TryCloseAsync();
                        vmFactory.Release(clipToRemove);
                    }
                    break;

                case ClipEventType.ToggleFavorite:
                    ClipViewModel clipVm = Items.FirstOrDefault(vm => vm.Model.Id == message.Source.Id);
                    if (clipVm != null) {
                        ClipModel clip = clipVm.Model;
                        TagModel favoriteTag = clip.Tags.FirstOrDefault(
                            tag => tag.TypeName == "category" && tag.Value.ToString() == "favorite"
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
