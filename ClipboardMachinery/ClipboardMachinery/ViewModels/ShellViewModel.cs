using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Bibliotheque.Machine;
using Caliburn.Micro;
using ClipboardMachinery.Events;
using ClipboardMachinery.Events.Collection;
using ClipboardMachinery.Models;

namespace ClipboardMachinery.ViewModels {

    internal class ShellViewModel : Conductor<object>, IShell,
        IHandle<ChangeAppVisiblity>, IHandle<SetViewFilter> {

        public IEventAggregator Events { get; }

        public BindableCollection<ClipViewModel> ClipboardItems { get; }

        public ICollectionView ClipboardItemsView { get; }

        public BindableCollection<SelectableActionModel> MenuItems { get; }

        public SelectableActionModel SelectedMenuItem => MenuItems.FirstOrDefault(model => model.IsSelected);

        [ImportingConstructor]
        public ShellViewModel(IEventAggregator eventAggregator) {
            Events = eventAggregator;
            Events.Subscribe(this);

            MenuItems = new BindableCollection<SelectableActionModel> {
                new SelectableActionModel(
                    name: "History",
                    iconName: "IconHistory",
                    action: () => ActivateItem(new HistoryViewModel(Events) {
                        Items = ClipboardItems
                    })
                ),
                new SelectableActionModel(
                    name: "Favorite",
                    iconName: "IconFavorite",
                    action: () => ActivateItem(new FavoritesViewModel(Events) {
                        Items = ClipboardItems
                    })
                ),
                new SelectableActionModel(
                    name: "Search",
                    iconName: "IconSearch",
                    action: () => ActivateItem(new SearchViewModel())
                )
            };

            ClipboardItems = new BindableCollection<ClipViewModel>();
            ClipboardItemsView = CollectionViewSource.GetDefaultView(ClipboardItems);

            if (!MenuItems.Any(m => m.IsSelected)) {
                SelectMenuItem(MenuItems.First());
            }

            ClipboardObserver.ClipboardChanged += (sender, args) => {
                if (args.Payload == string.Empty || args.Payload == ClipboardItems.FirstOrDefault()?.RawContent) {
                    return;
                }

                ClipboardItems.Insert(0, new ClipViewModel(
                    events: Events,
                    content: args.Payload,
                    timestamp: DateTime.UtcNow
                ));
            };

            new HotKey(Key.H, HotKey.KeyModifier.Ctrl, (hotKey) =>
                Events.PublishOnUIThread(new ChangeAppVisiblity(VisiblityChangeType.Toggle))
            );
        }

        public void SelectMenuItem(SelectableActionModel option) {
            if (option == SelectedMenuItem) {
                return;
            }

            foreach (SelectableActionModel barItemModel in MenuItems.Where(m => m.IsSelected)) {
                barItemModel.IsSelected = false;
            }

            ClipboardItemsView.Filter = null;
            option.IsSelected = true;
            option.Action?.Invoke();
            NotifyOfPropertyChange(() => SelectedMenuItem);
        }

        #region Event Handlers

        public void Handle(ChangeAppVisiblity message) {
            Window win = (Window)GetView();

            switch (message.EventVisiblityChangeType) {
                case VisiblityChangeType.Hide:
                    win.Hide();
                    return;

                case VisiblityChangeType.Show:
                    win.Show();
                    return;

                case VisiblityChangeType.Toggle:
                    if (win.IsVisible) {
                        win.Hide();
                    } else {
                        SelectMenuItem(SelectedMenuItem);
                        win.Show();
                    }
                    return;
            }
        }

        public void Handle(SetViewFilter message) {
            ClipboardItemsView.Filter = message.Filter;
        }

        #endregion

    }

}
