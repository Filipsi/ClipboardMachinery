using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.DialogOverlays.TagTypeEditor;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Pages.TagTypes {

    public class TagTypesViewModel : LazyPageBase<TagTypeViewModel, TagTypeModel>, IScreenPage, IHandle<TagEvent> {

        #region IScreenPage

        public string Title
            => "Tags";

        public string Icon
            => "IconTag";

        public byte Order
            => 3;

        #endregion

        #region Properties

        public bool CanCreateNew {
            get => canCreateNew;
            private set {
                if (canCreateNew == value) {
                    return;
                }

                canCreateNew = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IViewModelFactory vmFactory;
        private readonly IDialogOverlayFactory dialogOverlayFactory;

        private bool canCreateNew = true;

        #endregion

        public TagTypesViewModel(IEventAggregator eventAggregator, IDataRepository dataRepository, IViewModelFactory vmFactory, IDialogOverlayFactory dialogOverlayFactory)
            : base(dataRepository.CreateLazyTagTypeProvider(15)) {

            this.eventAggregator = eventAggregator;
            this.vmFactory = vmFactory;
            this.dialogOverlayFactory = dialogOverlayFactory;
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

        #region Actions

        public void CreateNew() {
            TagTypeModel newTagType = new TagTypeModel {
                Kind = typeof(string)
            };

            TagTypeEditorViewModel tagTypeEditor = dialogOverlayFactory.CreateTagTypeEditor(newTagType, isCreatingNew: true);
            tagTypeEditor.Deactivated += OnTagTypeEditorDeactivated;
            CanCreateNew = false;
            eventAggregator.PublishOnCurrentThreadAsync(DialogOverlayEvent.Open(tagTypeEditor));
        }

        #endregion

        #region Handlers

        private void OnTagTypeEditorDeactivated(object sender, DeactivationEventArgs e) {
            TagTypeEditorViewModel tagTypeEditor = (TagTypeEditorViewModel)sender;
            tagTypeEditor.Deactivated -= OnTagTypeEditorDeactivated;
            dialogOverlayFactory.Release(tagTypeEditor);
            CanCreateNew = true;
        }

        public async Task HandleAsync(TagEvent message, CancellationToken cancellationToken) {
            if (message.EventType != TagEvent.TagEventType.TypeAdded) {
                return;
            }

            await Reset();
        }

        #endregion

    }

}
