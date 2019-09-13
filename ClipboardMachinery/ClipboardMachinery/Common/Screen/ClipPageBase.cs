using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Core;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Pages;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Common.Screen {

    public abstract class ClipPageBase : LazyPageBase<ClipViewModel, ClipModel>, IHandle<ClipEvent> {

        #region Fields

        protected readonly IDataRepository dataRepository;
        protected readonly IEventAggregator eventAggregator;
        protected readonly IViewModelFactory vmFactory;

        #endregion

        protected ClipPageBase(int batchSize, IDataRepository dataRepository, IEventAggregator eventAggregator, IViewModelFactory vmFactory)
            : base(dataRepository.CreateLazyClipProvider(batchSize)) {

            this.dataRepository = dataRepository;
            this.vmFactory = vmFactory;
            this.eventAggregator = eventAggregator;
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
                case ClipEvent.ClipEventType.Created:
                    if (!IsAllowedAddClipsFromKeyboard(message)) {
                        return;
                    }

                    ClipViewModel createdClip = vmFactory.CreateClip(message.Source);
                    Items.Insert(0, createdClip);
                    await ActivateItemAsync(createdClip, cancellationToken);
                    OnKeyboardClipAdded(createdClip);
                    break;

                case ClipEvent.ClipEventType.Remove:
                    ClipViewModel clipToRemove = Items.FirstOrDefault(vm => vm.Model.Id == message.Source.Id);
                    if (clipToRemove != null) {
                        await Task.Run(() => dataRepository.DeleteClip(clipToRemove.Model.Id), cancellationToken);
                        await clipToRemove.TryCloseAsync();
                        vmFactory.Release(clipToRemove);
                    }
                    break;
            }
        }

        protected virtual void OnKeyboardClipAdded(ClipViewModel newClip) {
        }

        #endregion

    }

}
