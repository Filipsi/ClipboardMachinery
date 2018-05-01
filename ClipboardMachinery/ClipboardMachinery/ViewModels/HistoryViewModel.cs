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
using Ninject;
using Screen = Caliburn.Micro.Screen;

namespace ClipboardMachinery.ViewModels {

    internal class HistoryViewModel : Screen,
        IHandle<ItemRemoved<ClipViewModel>>, IHandle<ItemSelected<ClipViewModel>> {

        [Inject]
        public IEventAggregator Events { set; get; }

        [Inject]
        public IClipboardItemsProvider ClipboardItemsProvider { set; get; }

        public bool ErrorMessageIsVisible => ClipboardItemsProvider.Items?.Count == 0;

        protected override void OnInitialize() {
            base.OnInitialize();

            Events.Subscribe(this);

            ClipboardItemsProvider.Items.CollectionChanged += ItemsInCollectionChanged;
            ItemsInCollectionChanged(ClipboardItemsProvider.Items, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ClipboardItemsProvider.Items));
        }

        protected virtual void ItemsInCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    if (((BindableCollection<ClipViewModel>) sender).Count < 2) {
                        NotifyOfPropertyChange(() => ErrorMessageIsVisible);
                    }
                    return;
            }
        }

        #region Event Handlers

        public void Handle(ItemRemoved<ClipViewModel> message) {
            if (!IsActive) return;
            ClipboardItemsProvider.Items.Remove(message.Item);
        }

        public void Handle(ItemSelected<ClipViewModel> message) {
            if (!IsActive) return;
            ClipboardObserver.IgnoreNextChange(message.Item.RawContent);

            try {
                switch (message.Item.Type) {
                    case ClipViewModel.EntryType.Image:
                        Clipboard.SetImage(((Image)message.Item.Content).Source as BitmapImage ?? throw new InvalidOperationException());
                        break;

                    default:
                        Clipboard.SetText(message.Item.RawContent);
                        break;
                }
            } catch (Exception) {
                // ignored
            }

            Events.PublishOnUIThread(new ChangeAppVisiblity(VisiblityChangeType.Hide));
        }

        #endregion

    }

}
