using System;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Core.Data;
using ClipboardMachinery.Core.Data.LazyProvider;
using ClipboardMachinery.Plumbing.Factories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace ClipboardMachinery.Pages {

    public abstract class LazyClipPage : ClipPageBase {

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
                    loadBatchTask = Task.Run(LoadClipBatch);
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

        private bool IsLoadingBatch
            => loadBatchTask?.IsCompleted == false;

        #endregion

        #region Fields

        protected readonly int batchSize;
        protected readonly ILazyDataProvider lazyClipProvider;

        private double remainingScrollableHeight;
        private double verticalScrollOffset;
        private Task loadBatchTask;

        protected bool clearAllItemsOnDeactivate = false;

        #endregion

        protected LazyClipPage(int batchSize, IDataRepository dataRepository, IClipViewModelFactory clipVmFactory) : base(dataRepository, clipVmFactory) {
            this.batchSize = batchSize;
            lazyClipProvider = dataRepository.CreateLazyClipProvider(batchSize);
            loadBatchTask = Task.Run(LoadClipBatch);
        }

        #region Logic

        private async Task LoadClipBatch() {
            foreach (ClipModel model in await lazyClipProvider.GetNextBatchAsync<ClipModel>()) {
                ActivateItem(clipVmFactory.Create(model));
            }
        }

        #endregion

        #region Handlers

        protected override void OnDeactivate(bool close) {
            // This is done mainly for optimization and reset when pages are switched
            // to prevent from having outdated clips or large amounts of then lingering on the page
            // Also used when screen is closed to release all clips
            IEnumerable<ClipViewModel> itemsToRemove = close || clearAllItemsOnDeactivate
                ? Items
                : Items.Skip(batchSize);

            foreach (ClipViewModel clip in itemsToRemove.ToArray()) {
                clip.TryClose(true);
                clipVmFactory.Release(clip);
            }

            lazyClipProvider.Offset = Items.Count;
            VerticalScrollOffset = 0;

            base.OnDeactivate(close);
        }

        protected override void OnActivate() {
            // Initial item load after page activates.
            // This logic was moved here from RemainingScrollableHeight property, to prevent item pre-loading.
            if (!IsLoadingBatch && Items.Count == 0) {
                loadBatchTask = Task.Run(LoadClipBatch);
            }

            base.OnActivate();
        }

        #endregion

    }

}
