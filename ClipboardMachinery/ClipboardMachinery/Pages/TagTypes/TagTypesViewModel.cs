using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.Core.Data;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Pages.TagTypes {

    public class TagTypesViewModel : LazyPageBase<TagTypeViewModel, TagTypeModel>, IScreenPage {

        #region IScreenPage

        public string Title
            => "Tags";

        public string Icon
            => "IconTag";

        public byte Order
            => 3;

        #endregion

        #region Fields

        private readonly IViewModelFactory vmFactory;

        #endregion

        public TagTypesViewModel(IDataRepository dataRepository, IViewModelFactory vmFactory)
            : base(dataRepository.CreateLazyTagTypeProvider(15)) {

            this.vmFactory = vmFactory;
        }

        #region Logic

        protected override bool IsClearingItemsWhenDeactivating() {
            return true;
        }

        protected override TagTypeViewModel CreateItem(TagTypeModel model) {
            return vmFactory.CreateTagType(model);
        }

        protected override void ReleaseItem(TagTypeViewModel instance) {
            vmFactory.Release(instance);
        }

        #endregion

    }

}
