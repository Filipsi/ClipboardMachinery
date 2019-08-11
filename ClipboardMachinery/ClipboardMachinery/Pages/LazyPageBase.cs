using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClipboardMachinery.Core.DataStorage;
using Nito.Mvvm;

namespace ClipboardMachinery.Pages {

    public abstract class LazyPageBase<TVM, TM> : Conductor<TVM>.Collection.AllActive where TVM : class, IScreen where TM : class {

        #region Properties

        public double RemainingScrollableHeight {
            get => remainingScrollableHeight;
            set {
                // Floating point tolerance
                if (Math.Abs(remainingScrollableHeight - value) < 4) {
                    return;
                }

                remainingScrollableHeight = value;
                NotifyOfPropertyChange();

                // Handle infinite scrolling
                // Load new batch of item when user scrolls to the bottom of a page.
                // Initial load is handed by OnActivate method.
                if (IsActive && DataLoading?.IsNotCompleted != true && remainingScrollableHeight < 16) {
                    DataLoading = NotifyTask.Create(Task.Run(LoadDataBatch));
                }
            }
        }

        public double VerticalScrollOffset {
            get => verticalScrollOffset;
            set {
                // Floating point tolerance
                if (Math.Abs(verticalScrollOffset - value) < 4) {
                    return;
                }

                verticalScrollOffset = value;
                NotifyOfPropertyChange();
            }
        }

        public NotifyTask DataLoading {
            get => dataLoading;
            set {
                if (dataLoading != value) {
                    return;
                }

                dataLoading = value;
                NotifyOfPropertyChange();
            }
        }

        protected ILazyDataProvider DataProvider {
            get;
        }

        public bool WatermarkIsVisible
            => Items.Count == 0;

        #endregion

        #region Fields

        private double remainingScrollableHeight;
        private double verticalScrollOffset;
        private NotifyTask dataLoading;

        #endregion

        protected LazyPageBase(ILazyDataProvider dataProvider) {
            Items.CollectionChanged += OnItemsCollectionChanged;
            DataProvider = dataProvider;
        }

        #region Exposed logic

        protected abstract TVM CreateItem(TM model);

        protected abstract void ReleaseItem(TVM instance);

        protected abstract bool IsClearingItemsWhenDeactivating();

        protected async Task Reset() {
            await ClearAllItems(true);
            StartLoadingBatch(CancellationToken.None);
        }

        #endregion

        #region Logic

        private async Task LoadDataBatch() {
            foreach (TM model in await DataProvider.GetNextBatchAsync<TM>()) {
                await ActivateItemAsync(CreateItem(model), CancellationToken.None);
            }
        }

        private void StartLoadingBatch(CancellationToken cancellationToken) {
            if (DataLoading?.IsNotCompleted != true && Items.Count == 0) {
                DataLoading = NotifyTask.Create(Task.Run(LoadDataBatch, cancellationToken));
            }
        }

        private async Task ClearAllItems(bool close) {
            // This is done mainly for optimization and reset when pages are switched
            // to prevent from having outdated clips or large amounts of then lingering on the page
            // Also used when screen is closed to release all clips
            IEnumerable<TVM> itemsToRemove = close || IsClearingItemsWhenDeactivating()
                ? Items
                : Items.Skip(DataProvider.BatchSize);

            foreach (TVM item in itemsToRemove.ToArray()) {
                await item.TryCloseAsync(true);
                ReleaseItem(item);
            }

            DataProvider.Offset = Items.Count;
            VerticalScrollOffset = 0;
        }

        #endregion

        #region Handlers

        protected override Task OnActivateAsync(CancellationToken cancellationToken) {
            // Initial item load after page activates.
            // This logic was moved here from RemainingScrollableHeight property, to prevent item pre-loading.
            StartLoadingBatch(cancellationToken);
            return Task.CompletedTask;
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            if (close) {
                Items.CollectionChanged -= OnItemsCollectionChanged;
            }

            await ClearAllItems(close);
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    NotifyOfPropertyChange(() => WatermarkIsVisible);
                    break;
            }
        }

        #endregion

    }
}
