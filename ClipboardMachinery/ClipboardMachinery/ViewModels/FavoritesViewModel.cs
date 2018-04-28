using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ClipboardMachinery.ViewModels {

    internal class FavoritesViewModel : HistoryViewModel {

        public bool FavoritesEmptyErrorMessageIsVisible => !Items.Any(model => model.IsFavorite);

        public ICollectionView ItemsView {
            get => _itemsView;
            internal set {
                if (Equals(value, _itemsView)) return;
                _itemsView = value;
                ApplyItemFilter();
                NotifyOfPropertyChange(() => ItemsView);
            }
        }

        private ICollectionView _itemsView;

        private void ApplyItemFilter() {
            ItemsView.Filter = model => ((HistoryEntryViewModel)model).IsFavorite;
        }

        protected override void ItemsInCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            base.ItemsInCollectionChanged(sender, e);

            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems.Cast<HistoryEntryViewModel>().Any(model => model.IsFavorite)) {
                        NotifyOfPropertyChange(() => FavoritesEmptyErrorMessageIsVisible);
                    }
                    foreach (HistoryEntryViewModel model in e.NewItems) {
                        model.PropertyChanged += OnItemPropertyChanged;
                    }
                    return;

                case NotifyCollectionChangedAction.Remove:
                    if (FavoritesEmptyErrorMessageIsVisible) {
                        NotifyOfPropertyChange(() => FavoritesEmptyErrorMessageIsVisible);
                    }
                    foreach (HistoryEntryViewModel model in e.NewItems) {
                        model.PropertyChanged -= OnItemPropertyChanged;
                    }
                    return;
            }
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName != nameof(HistoryEntryViewModel.IsFavorite)) return;
            if (((HistoryEntryViewModel) sender).IsFavorite) return;

            ApplyItemFilter();
            if (FavoritesEmptyErrorMessageIsVisible) {
                NotifyOfPropertyChange(() => FavoritesEmptyErrorMessageIsVisible);
            }
        }
    }

}
