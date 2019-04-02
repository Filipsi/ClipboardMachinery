using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Clip;
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
                    ClipViewModel clip = Items.FirstOrDefault(vm => vm.Model == message.Source);
                    if (clip != null) {
                        Items.Remove(clip);
                        await dataRepository.DeleteClip(clip.Model.Id);
                        clipVmFactory.Release(clip);
                    }
                    break;
            }
        }

        protected virtual void OnKeyboardClipAdded(ClipViewModel newClip) {
        }

        #endregion

    }

}
