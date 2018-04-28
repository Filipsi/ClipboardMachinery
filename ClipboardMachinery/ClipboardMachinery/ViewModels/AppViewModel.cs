using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Bibliotheque.Machine;
using Caliburn.Micro;
using ClipboardMachinery.Models;

namespace ClipboardMachinery.ViewModels {

    internal class AppViewModel : Conductor<object> {

        public BindableCollection<HistoryEntryViewModel> ClipboardItems { get; }

        public ICollectionView ClipboardItemsView { get; }

        public BindableCollection<SelectableActionModel> ActionItems { get; }

        public SelectableActionModel SelectedActionItem => ActionItems.FirstOrDefault(model => model.IsSelected);

        public AppViewModel() {
            ActionItems = new BindableCollection<SelectableActionModel> {
                new SelectableActionModel(
                    name: "History",
                    iconName: "IconHistory",
                    action: () => ActivateItem(new HistoryViewModel {
                        Items = ClipboardItems,
                        HideAppHandler = HideApp
                    })
                ),
                new SelectableActionModel(
                    name: "Favorite",
                    iconName: "IconFavorite",
                    action: () => {
                        ActivateItem(new FavoritesViewModel {
                            Items = ClipboardItems,
                            ItemsView = ClipboardItemsView,
                            HideAppHandler = HideApp
                        });
                    }
                ),
                new SelectableActionModel(
                    name: "Search",
                    iconName: "IconSearch",
                    action: () => ActivateItem(new SearchViewModel())
                )
            };

            ClipboardItems = new BindableCollection<HistoryEntryViewModel>();
            ClipboardItemsView = CollectionViewSource.GetDefaultView(ClipboardItems);

            if (!ActionItems.Any(m => m.IsSelected)) {
                Select(ActionItems.First());
            }

            ClipboardObserver.ClipboardChanged += (sender, args) => {
                if (args.Payload == string.Empty || args.Payload == ClipboardItems.FirstOrDefault()?.RawContent) {
                    return;
                }

                ClipboardItems.Insert(0, new HistoryEntryViewModel(
                    content: args.Payload,
                    timestamp: DateTime.UtcNow
                ));
            };

            new HotKey(Key.H, HotKey.KeyModifier.Ctrl, (hotKey) => ToggleAppVisiblity());
        }

        public void Select(SelectableActionModel option) {
            foreach (SelectableActionModel barItemModel in ActionItems.Where(m => m.IsSelected)) {
                barItemModel.IsSelected = false;
            }

            ClipboardItemsView.Filter = null;
            option.IsSelected = true;
            option.Action?.Invoke();

            NotifyOfPropertyChange(nameof(SelectedActionItem));
        }

        public void ToggleAppVisiblity() {
            Window win = (Window) GetView();

            if (win.IsVisible) {
                HideApp();
            } else {
                // This is done in order to reset scrollbar position (not necessary, but nice)
                Select(SelectedActionItem);
                win.Show();
            }
        }

        public void HideApp() => (GetView() as Window)?.Hide();

    }

}
