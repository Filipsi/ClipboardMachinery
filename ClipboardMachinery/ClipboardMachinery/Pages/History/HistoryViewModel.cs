using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Core.Repository;
using ClipboardMachinery.Core.Repository.LazyProvider;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Pages.History {

    public class HistoryViewModel : ClipPageBase, IScreenPage {

        #region IPage

        public string Title
            => "History";

        public string Icon
            => "IconHistory";

        public byte Order
            => 1;

        #endregion

        #region Properties

        public double RemainingScrollableHeight {
            get => remainingScrollableHeight;
            set {
                if (remainingScrollableHeight == value) {
                    return;
                }

                remainingScrollableHeight = value;
                NotifyOfPropertyChange();

                if (!IsLoadingHistory && remainingScrollableHeight < 16) {
                    loadHistoryTask = Task.Run(LoadClipBatch);
                }
            }
        }

        public double VerticalScrollOffset {
            get => verticalScrollOffset;
            set {
                if (verticalScrollOffset == value) {
                    return;
                }

                verticalScrollOffset = value;
                NotifyOfPropertyChange();
            }
        }

        private bool IsLoadingHistory
            => loadHistoryTask?.Status == TaskStatus.Running;

        #endregion

        #region Fields

        private static readonly int batchSize = 15;
        private readonly ILazyDataProvider lazyClipProvider;

        private double remainingScrollableHeight;
        private double verticalScrollOffset;
        private Task loadHistoryTask;

        #endregion

        public HistoryViewModel(IDataRepository dataRepository, IClipViewModelFactory clipVmFactory) : base(dataRepository, clipVmFactory) {
            lazyClipProvider = dataRepository.CreateLazyClipProvider(batchSize);
            loadHistoryTask = Task.Run(LoadClipBatch);
        }

        #region Logic

        private async Task LoadClipBatch() {
            foreach (ClipModel model in await lazyClipProvider.GetNextBatchAsync<ClipModel>()) {
                Items.Add(clipVmFactory.Create(model));
            }
        }

        #endregion

        #region Handlers

        protected override void OnKeyboardClipAdded(ClipViewModel newClip) {
            base.OnKeyboardClipAdded(newClip);

            // When new clip is added and user is not scrolling, we try to keep loaded clip count at size of one batch
            // This prevents from having too many items slowing down deactivation and switching between pages
            if (Items.Count > batchSize && VerticalScrollOffset == 0) {
                ClipViewModel lastClip = Items.Last();
                Items.Remove(lastClip);
                clipVmFactory.Release(lastClip);
            }
        }

        protected override void OnDeactivate(bool close) {
            base.OnDeactivate(close);

            lazyClipProvider.Reset();
            VerticalScrollOffset = 0;
            foreach (ClipViewModel clip in Items.Skip(close ? 0 : batchSize).ToArray()) {
                Items.Remove(clip);
                clipVmFactory.Release(clip);
            }
        }

        #endregion

    }

}
