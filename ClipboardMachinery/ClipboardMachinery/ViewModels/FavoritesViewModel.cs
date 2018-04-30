using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using ClipboardMachinery.Events;
using ClipboardMachinery.Events.Collection;

namespace ClipboardMachinery.ViewModels {

    internal class FavoritesViewModel : HistoryViewModel, IHandle<ItemFavoriteChanged<ClipViewModel>> {

        public new bool ErrorMessageIsVisible => !Items.Any(model => model.IsFavorite);

        public FavoritesViewModel(IEventAggregator events) : base(events)
            => ApplyItemFilter();

        private void ApplyItemFilter() {
            Events.PublishOnUIThread(new SetViewFilter(
                model => ((ClipViewModel)model).IsFavorite
            ));
        }

        #region Event Handlers

        public void Handle(ItemFavoriteChanged<ClipViewModel> message) {
            if (message.Item.IsFavorite) return;

            ApplyItemFilter();
            if (Items.Count > 0) {
                NotifyOfPropertyChange(() => ErrorMessageIsVisible);
            }
        }

        #endregion


    }

}
