using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using ClipboardMachinery.Events;
using ClipboardMachinery.Events.Collection;
using Ninject;

namespace ClipboardMachinery.ViewModels {

    internal class FavoritesViewModel : HistoryViewModel, IHandle<ItemFavoriteChanged<ClipViewModel>> {

        public new bool ErrorMessageIsVisible => !ClipboardItemsProvider.Items.Any(model => model.IsFavorite);

        protected override void OnInitialize() {
            base.OnInitialize();
            ApplyItemFilter();
        }

        private void ApplyItemFilter() {
            Events.PublishOnUIThread(new SetViewFilter(
                model => ((ClipViewModel)model).IsFavorite
            ));
        }

        #region Event Handlers

        public void Handle(ItemFavoriteChanged<ClipViewModel> message) {
            if (!IsActive) return;
            if (message.Item.IsFavorite) return;

            ApplyItemFilter();
            if (ClipboardItemsProvider.Items.Count > 0) {
                NotifyOfPropertyChange(() => ErrorMessageIsVisible);
            }
        }

        #endregion


    }

}
