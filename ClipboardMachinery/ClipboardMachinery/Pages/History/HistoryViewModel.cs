using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Core.Repositories;
using ClipboardMachinery.Core.Repositories.Lazy;
using Screen = Caliburn.Micro.Screen;

namespace ClipboardMachinery.Pages.History {

    public class HistoryViewModel : Screen, IScreenPage {

        #region IPage

        public string Title
            => "History";

        public string Icon
            => "IconHistory";

        public byte Order
            => 1;

        #endregion

        #region Properties

        public bool ErrorMessageIsVisible
            => ClipboardItems.Count == 0;

        public BindableCollection<ClipViewModel> ClipboardItems {
            get;
        }

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

                    Task
                        .Run(LoadClipBatch)
                        .ContinueWith((t) => loadLock = false);
                }
            }
        }

        #endregion

        #region Fields

        private readonly Func<ClipViewModel> clipVmFactory;
        private readonly ILazyDataProvider lazyClipProvider;

        private double remainingScrollableHeight;
        private bool loadLock = false;

        #endregion

        public HistoryViewModel(IDataRepository dataRepository, Func<ClipViewModel> clipVmFactory) {
            ClipboardItems = new BindableCollection<ClipViewModel>();
            ClipboardItems.CollectionChanged += OnClipboardItemsCollectionChanged;
            this.clipVmFactory = clipVmFactory;

            lazyClipProvider = dataRepository.CreateLazyClipProvider(batchSize: 15);
            Task.Run(LoadClipBatch);
        }

        #region Logic

        private async Task LoadClipBatch() {
            foreach (ClipModel model in await lazyClipProvider.GetNextBatchAsync<ClipModel>()) {
                ClipViewModel vm = clipVmFactory.Invoke();
                vm.Model = model;
                ClipboardItems.Add(vm);
            }
        }

        #endregion

        #region Handlers

        private void OnClipboardItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    NotifyOfPropertyChange(() => ErrorMessageIsVisible);
                    break;
            }
        }

        #endregion

    }

}
