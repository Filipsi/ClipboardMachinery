using System.Collections.Specialized;
using Caliburn.Micro;
using ClipboardMachinery.Core.Services.Clipboard;
using ClipboardMachinery.Events;
using ClipboardMachinery.Plumbing;
using Screen = Caliburn.Micro.Screen;

namespace ClipboardMachinery.ViewModels {

    public class HistoryViewModel : Screen, IHandle<ItemSelected<ClipViewModel>> {

        #region Properties

        public bool ErrorMessageIsVisible
            => shell.ClipboardItems?.Count == 0;

        public IObservableCollection<ClipViewModel> ClipboardItems
            => shell.ClipboardItems;

        #endregion

        #region Fields

        protected readonly IEventAggregator eventBus;
        protected readonly IClipboardService clipboard;
        protected readonly IShell shell;

        #endregion

        public HistoryViewModel(IEventAggregator eventAggregator, IClipboardService clipboardService, IShell shellVm) {
            eventBus = eventAggregator;
            clipboard = clipboardService;
            shell = shellVm;
        }

        #region Handlers

        protected override void OnInitialize() {
            base.OnInitialize();

            shell.ClipboardItems.CollectionChanged += ItemsInCollectionChanged;
            ItemsInCollectionChanged(shell.ClipboardItems, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, shell.ClipboardItems));
        }

        protected virtual void ItemsInCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            NotifyOfPropertyChange(() => ErrorMessageIsVisible);
        }
        public void Handle(ItemSelected<ClipViewModel> message) {
            if (!IsActive) {
                return;
            }

            clipboard.IgnoreNextChange(message.Item.Model.RawContent);

            try {
                switch (message.Item.Type) {
                    case ClipViewModel.EntryType.Image:
                        clipboard.SetClipboardContent(message.Item.Content);
                        break;

                    default:
                        clipboard.SetClipboardContent(message.Item.Model.RawContent);
                        break;
                }
            } catch {}

            shell.IsVisible = false;
        }

        #endregion

    }

}
