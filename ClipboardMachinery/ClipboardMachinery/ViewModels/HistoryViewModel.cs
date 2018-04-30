using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Bibliotheque.Machine;
using Caliburn.Micro;
using ClipboardMachinery.Events;
using ClipboardMachinery.Events.Collection;
using Screen = Caliburn.Micro.Screen;

namespace ClipboardMachinery.ViewModels {

    internal class HistoryViewModel : Screen,
        IHandle<ItemRemoved<ClipViewModel>>, IHandle<ItemSelected<ClipViewModel>> {

        public BindableCollection<ClipViewModel> Items {
            get => _items;
            set {
                if (Equals(value, _items)) return;

                if (Items != null) {
                    Items.CollectionChanged -= ItemsInCollectionChanged;
                    ItemsInCollectionChanged(_items, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _items));
                }

                _items = value;
                _items.CollectionChanged += ItemsInCollectionChanged;
                ItemsInCollectionChanged(_items, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _items));
                NotifyOfPropertyChange(() => Items);
            }
        }

        public bool ErrorMessageIsVisible => Items?.Count == 0;

        protected readonly IEventAggregator Events;
        private BindableCollection<ClipViewModel> _items;

        public HistoryViewModel(IEventAggregator events) {
            Events = events;
            Events.Subscribe(this);
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
            Items.Remove(message.Item);
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
