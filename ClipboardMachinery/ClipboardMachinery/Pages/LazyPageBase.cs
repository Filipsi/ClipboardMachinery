using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClipboardMachinery.Core.Data.LazyProvider;

namespace ClipboardMachinery.Pages {

    public abstract class LazyPage<TVM, TM> : Conductor<TVM>.Collection.AllActive where TVM : class, IScreen where TM : class {

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
                if (IsActive && !IsLoadingBatch && remainingScrollableHeight < 16) {
                    loadBatchTask = Task.Run(LoadDataBatch);
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

        protected ILazyDataProvider DataProvider {
            get;
        }

        private bool IsLoadingBatch
            => loadBatchTask?.IsCompleted == false;

        public bool WatermarkIsVisible
            => Items.Count == 0;

        #endregion

        #region Fields

        private double remainingScrollableHeight;
        private double verticalScrollOffset;
        private Task loadBatchTask;

        #endregion

        protected LazyPage(ILazyDataProvider dataProvider) {
            Items.CollectionChanged += OnItemsCollectionChanged;
            DataProvider = dataProvider;
        }

        #region Logic

        private async Task LoadDataBatch() {
            foreach (TM model in await DataProvider.GetNextBatchAsync<TM>()) {
                await ActivateItemAsync(CreateItem(model), CancellationToken.None);
            }
        }

        protected abstract TVM CreateItem(TM model);

        protected abstract void ReleaseItem(TVM instance);

        protected abstract bool IsClearingItemsWhenDeactivating(bool close);


        #endregion

        #region Handlers

        protected override async Task OnActivateAsync(CancellationToken cancellationToken) {
            // Initial item load after page activates.
            // This logic was moved here from RemainingScrollableHeight property, to prevent item pre-loading.
            if (!IsLoadingBatch && Items.Count == 0) {
                await Task.Run(LoadDataBatch, cancellationToken);
            }
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            if (close) {
                Items.CollectionChanged -= OnItemsCollectionChanged;
            }

            // This is done mainly for optimization and reset when pages are switched
            // to prevent from having outdated clips or large amounts of then lingering on the page
            // Also used when screen is closed to release all clips
            IEnumerable<TVM> itemsToRemove = close || IsClearingItemsWhenDeactivating(close)
                ? Items
                : Items.Skip(DataProvider.BatchSize);

            foreach (TVM item in itemsToRemove.ToArray()) {
                await item.TryCloseAsync(true);
                ReleaseItem(item);
            }

            DataProvider.Offset = Items.Count;
            VerticalScrollOffset = 0;
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
