using System;
using System.Collections.Generic;
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
                    Task.Run(LoadClipBatch).ContinueWith((t) => loadLock = false);
                }
            }
        }

        #endregion

        #region Fields

        private readonly Func<ClipViewModel> clipVmFactory;
        private readonly ILazyDataProvider lazyClipProvider;
        private readonly IDataRepository dataRepository;

        private double remainingScrollableHeight;
        private bool loadLock = false;

        #endregion

        public HistoryViewModel(IDataRepository dataRepository, Func<ClipViewModel> clipVmFactory) {
            this.dataRepository = dataRepository;
            this.clipVmFactory = clipVmFactory;

            ClipboardItems = new BindableCollection<ClipViewModel>();
            ClipboardItems.CollectionChanged += OnClipboardItemsCollectionChanged;

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

        private async void OnClipRemovalRequest(object sender, EventArgs e) {
            ClipViewModel clipVm = sender as ClipViewModel;
            ClipboardItems.Remove(clipVm);
            await dataRepository.DeleteClip(clipVm.Model.Id);
        }

        private void OnClipboardItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    NotifyOfPropertyChange(() => ErrorMessageIsVisible);
                    foreach (ClipViewModel clipVm in e.NewItems) {
                        clipVm.RemovalRequest += OnClipRemovalRequest;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    NotifyOfPropertyChange(() => ErrorMessageIsVisible);
                    foreach (ClipViewModel clipVm in e.OldItems) {
                        clipVm.RemovalRequest -= OnClipRemovalRequest;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    NotifyOfPropertyChange(() => ErrorMessageIsVisible);
                    foreach (ClipViewModel clipVm in sender as ICollection<ClipViewModel>) {
                        clipVm.RemovalRequest -= OnClipRemovalRequest;
                    }
                    break;
            }
        }

        #endregion

    }

}
