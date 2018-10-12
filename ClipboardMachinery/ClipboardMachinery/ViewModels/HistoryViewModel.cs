using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Bibliotheque.Machine;
using Caliburn.Micro;
using ClipboardMachinery.Events;
using ClipboardMachinery.Events.Collection;
using ClipboardMachinery.Logic.ClipboardItemsProvider;
using Screen = Caliburn.Micro.Screen;

namespace ClipboardMachinery.ViewModels {

    public class HistoryViewModel : Screen, IHandle<ItemSelected<ClipViewModel>> {

        #region Properties

        public IClipboardItemsProvider ClipboardItemsProvider {
            get;
        }

        public bool ErrorMessageIsVisible
            => ClipboardItemsProvider.Items?.Count == 0;

        #endregion

        #region Fields

        protected readonly IEventAggregator eventBus;

        #endregion

        public HistoryViewModel(IClipboardItemsProvider clipboardItemsProvider, IEventAggregator eventAggregator) {
            ClipboardItemsProvider = clipboardItemsProvider;
            eventBus = eventAggregator;
        }

        #region Handlers

        protected override void OnInitialize() {
            base.OnInitialize();

            ClipboardItemsProvider.Items.CollectionChanged += ItemsInCollectionChanged;
            ItemsInCollectionChanged(ClipboardItemsProvider.Items, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ClipboardItemsProvider.Items));
        }

        protected virtual void ItemsInCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    NotifyOfPropertyChange(() => ErrorMessageIsVisible);
                    return;
            }
        }
        public void Handle(ItemSelected<ClipViewModel> message) {
            if (!IsActive) {
                return;
            }

            ClipboardObserver.IgnoreNextChange(message.Item.Model.RawContent);

            try {
                switch (message.Item.Type) {
                    case ClipViewModel.EntryType.Image:
                        Clipboard.SetImage(((Image)message.Item.Content).Source as BitmapImage ?? throw new InvalidOperationException());
                        break;

                    default:
                        Clipboard.SetText(message.Item.Model.RawContent);
                        break;
                }
            } catch {}

            eventBus.PublishOnUIThread(new ChangeAppVisiblity(VisiblityChangeType.Hide));
        }

        #endregion

    }

}
