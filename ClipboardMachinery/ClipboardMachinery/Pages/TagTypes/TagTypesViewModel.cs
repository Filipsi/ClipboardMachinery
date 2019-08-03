using System;
using System.Windows;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Plumbing.Factories;
using ClipboardMachinery.Popup.TagEditor;
using ClipboardMachinery.Popup.TagTypeEditor;

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
        private readonly IPopupFactory popupFactory;

        private bool canCreateNew = true;

        #endregion

        public TagTypesViewModel(IEventAggregator eventAggregator, IDataRepository dataRepository, IViewModelFactory vmFactory, IPopupFactory popupFactory)
            : base(dataRepository.CreateLazyTagTypeProvider(15)) {

            this.eventAggregator = eventAggregator;
            this.vmFactory = vmFactory;
            this.popupFactory = popupFactory;
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
            TagTypeModel newTagType = new TagTypeModel();
            TagTypeEditorViewModel tagTypeEditor = popupFactory.CreateTagTypeEditor(newTagType, isCreatingNew: true);
            tagTypeEditor.Deactivated += OnTagTypeEditorDeactivated;
            CanCreateNew = false;
            eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Show(tagTypeEditor));
        }

        #endregion

        #region Handlers

        private void OnTagTypeEditorDeactivated(object sender, DeactivationEventArgs e) {
            TagTypeEditorViewModel tagTypeEditor = (TagTypeEditorViewModel)sender;
            tagTypeEditor.Deactivated -= OnTagTypeEditorDeactivated;
            popupFactory.Release(tagTypeEditor);
            CanCreateNew = true;
        }

        #endregion

    }

}
