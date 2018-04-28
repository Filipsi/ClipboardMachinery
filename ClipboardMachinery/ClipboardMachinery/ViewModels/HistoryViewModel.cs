using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Bibliotheque.Machine;
using Caliburn.Micro;
using Action = System.Action;
using Screen = Caliburn.Micro.Screen;

namespace ClipboardMachinery.ViewModels {

    internal class HistoryViewModel : Screen {

        public Action HideAppHandler {
            get => _hideAppHandler;
            set {
                if (Equals(value, _hideAppHandler)) return;
                _hideAppHandler = value;
                NotifyOfPropertyChange(() => HideAppHandler);
            }
        }

        public BindableCollection<HistoryEntryViewModel> Items {
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

        public bool HistoryEmptyErrorMessageIsVisible => Items?.Count == 0;

        private BindableCollection<HistoryEntryViewModel> _items;
        private Action _hideAppHandler;

        protected virtual void ItemsInCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    NotifyOfPropertyChange(() => HistoryEmptyErrorMessageIsVisible);
                    foreach (HistoryEntryViewModel model in e.NewItems) {
                        model.Selected += HistoryItemSelected;
                        model.Destroyed += HistoryItemDestroyed;
                    }
                    return;

                case NotifyCollectionChangedAction.Remove:
                    NotifyOfPropertyChange(() => HistoryEmptyErrorMessageIsVisible);
                    foreach (HistoryEntryViewModel model in e.OldItems) {
                        model.Selected -= HistoryItemSelected;
                        model.Selected -= HistoryItemDestroyed;
                    }
                    return;
            }
        }

        private void HistoryItemSelected(object sender, EventArgs e) {
            HistoryEntryViewModel eventSender = (HistoryEntryViewModel) sender;
            ClipboardObserver.IgnoreNextChange(eventSender.RawContent);

            try {
                switch (eventSender.Type) {
                    case HistoryEntryViewModel.EntryType.Image:
                        Clipboard.SetImage(((Image)eventSender.Content).Source as BitmapImage ?? throw new InvalidOperationException());
                        break;

                    default:
                        Clipboard.SetText(eventSender.RawContent);
                        break;
                }
            } catch (Exception) {
                // ignored
            }

            HideAppHandler?.Invoke();
        }

        private void HistoryItemDestroyed(object sender, EventArgs e) {
            Items.Remove((HistoryEntryViewModel)sender);
        }

    }

}
