using System;
using System.Threading.Tasks;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Core.Repositories;
using ClipboardMachinery.Core.Repositories.Lazy;
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

                if (!loadLock && remainingScrollableHeight < 200) {
                    loadLock = true;
                    Task.Run(LoadClipBatch).ContinueWith((t) => loadLock = false);
                }
            }
        }

        #endregion

        #region Fields

        private readonly ILazyDataProvider lazyClipProvider;

        private double remainingScrollableHeight;
        private bool loadLock = false;

        #endregion

        public HistoryViewModel(IDataRepository dataRepository, IClipViewModelFactory clipVmFactory) : base(dataRepository, clipVmFactory) {
            lazyClipProvider = dataRepository.CreateLazyClipProvider(batchSize: 15);
            Task.Run(LoadClipBatch);
        }

        #region Logic

        private async Task LoadClipBatch() {
            foreach (ClipModel model in await lazyClipProvider.GetNextBatchAsync<ClipModel>()) {
                ClipboardItems.Add(clipVmFactory.Create(model));
            }
        }

        #endregion

    }

}
