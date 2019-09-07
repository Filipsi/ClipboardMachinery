using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Common.Screen;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Components.TagTypeLister {

    public class TagTypeListerViewModel : LazyPageBase<TagTypeViewModel, TagTypeModel>, IHandle<TagEvent> {

        #region Properties

        public TagTypeModel SelectedItem {
            get => selectedItem;
            private set {
                if (selectedItem == value) {
                    return;
                }

                selectedItem = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Fields

        private readonly IViewModelFactory vmFactory;
        private TagTypeModel selectedItem;

        #endregion

        public TagTypeListerViewModel(IDataRepository dataRepository, IViewModelFactory vmFactory) : base(dataRepository.CreateLazyTagTypeProvider(15)) {
            this.vmFactory = vmFactory;
        }

        #region Logic

        protected override bool IsClearingItemsWhenDeactivating() {
            return true;
        }

        protected override TagTypeViewModel CreateItem(TagTypeModel model) {
            TagTypeViewModel vm = vmFactory.CreateTagType(model);
            vm.Selected += OnTagTypeVmSelected;
            return vm;
        }

        protected override void ReleaseItem(TagTypeViewModel instance) {
            vmFactory.Release(instance);
        }

        private void OnTagTypeVmSelected(object sender, EventArgs e) {
            TagTypeViewModel selectedVm = (TagTypeViewModel) sender;
            SelectedItem = selectedVm.Model;
            OnItemSelected(selectedVm);
        }

        protected virtual void OnItemSelected(TagTypeViewModel item) {
        }

        #endregion

        #region Handlers

        public async Task HandleAsync(TagEvent message, CancellationToken cancellationToken) {
            switch (message.EventType) {
                case TagEvent.TagEventType.TypeAdded:
                case TagEvent.TagEventType.TypeRemoved:
                    await Reset();
                    break;
            }
        }

        #endregion

    }

}
