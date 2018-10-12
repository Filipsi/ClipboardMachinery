using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using ClipboardMachinery.Events;
using ClipboardMachinery.Events.Collection;
using ClipboardMachinery.FileSystem;
using ClipboardMachinery.Logic.ClipboardItemsProvider;

namespace ClipboardMachinery.ViewModels {

    public class FavoritesViewModel : HistoryViewModel, IHandle<ItemFavoriteChanged<ClipViewModel>> {

        public new bool ErrorMessageIsVisible
            => !ClipboardItemsProvider.Items.Any(vm => vm.Model.IsFavorite);

        public FavoritesViewModel(IEventAggregator eventAggregator, IClipboardItemsProvider clipboardItemsProvider) : base(clipboardItemsProvider, eventAggregator) {
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            ApplyItemFilter();
        }

        private void ApplyItemFilter() {
            eventBus.PublishOnUIThread(new SetViewFilter(
                vm => ((ClipViewModel)vm).Model.IsFavorite
            ));
        }

        #region Event Handlers

        public void Handle(ItemFavoriteChanged<ClipViewModel> message) {
            if (!IsActive) return;
            if (message.Item.Model.IsFavorite) return;

            ApplyItemFilter();
            if (ClipboardItemsProvider.Items.Count > 0) {
                NotifyOfPropertyChange(() => ErrorMessageIsVisible);
            }
        }

        #endregion


    }

}
