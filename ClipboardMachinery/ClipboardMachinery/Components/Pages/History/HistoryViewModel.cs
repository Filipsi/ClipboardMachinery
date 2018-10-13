using System;
using System.Collections.Specialized;
using Caliburn.Micro;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Core.Repositories;
using ClipboardMachinery.Core.Repositories.Lazy;
using ClipboardMachinery.Core.Services.Clipboard;
using ClipboardMachinery.Plumbing;
using Screen = Caliburn.Micro.Screen;

namespace ClipboardMachinery.Components.Pages.History {

    public class HistoryViewModel : Screen, IScreenPage {

        #region IPage

        public string Title => "History";

        public string Icon => "IconHistory";

        #endregion

        public bool ErrorMessageIsVisible { get; } = false;

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

                if (remainingScrollableHeight < 200) {
                    LoadNextClipBatch();
                }
            }
        }

        private readonly Func<ClipViewModel> clipVmFactory;
        private readonly ILazyDataProvider lazyClipProvider;

        private double remainingScrollableHeight;

        public HistoryViewModel(IDataRepository dataRepository, Func<ClipViewModel> clipVmFactory) {
            ClipboardItems = new BindableCollection<ClipViewModel>();
            this.clipVmFactory = clipVmFactory;

            lazyClipProvider = dataRepository.CreateLazyClipProvider(batchSize: 15);
            LoadNextClipBatch();
        }

        private void LoadNextClipBatch() {
            foreach (ClipModel model in lazyClipProvider.GetNextBatch<ClipModel>()) {
                ClipViewModel vm = clipVmFactory.Invoke();
                vm.Model = model;
                ClipboardItems.Add(vm);
            }
        }



    }

}
