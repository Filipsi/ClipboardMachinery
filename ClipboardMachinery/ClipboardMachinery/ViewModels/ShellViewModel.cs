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
using ClipboardMachinery.Logic;
using ClipboardMachinery.Logic.ClipboardItemsProvider;
using ClipboardMachinery.Logic.HotKeyHandler;
using ClipboardMachinery.Logic.ViewModelFactory;
using ClipboardMachinery.Models;
using Ninject;

namespace ClipboardMachinery.ViewModels {

    internal class ShellViewModel : Conductor<object>, IShell,
        IHandle<ChangeAppVisiblity>, IHandle<SetViewFilter> {

        [Inject]
        public IEventAggregator Events { set; get; }

        [Inject]
        public IViewModelFactory ViewModelFactory { set; get; }

        [Inject]
        public IClipboardItemsProvider ClipboardItemsProvider { set; get; }

        [Inject]
        public IHotKeyHandler HotKeyHandler { set; get; }

        public double                                    AppWidth           { get; } = SystemParameters.PrimaryScreenWidth / 3;
        public double                                    MaxContentHeight   { get; } = SystemParameters.PrimaryScreenHeight / 1.5;
        public BindableCollection<SelectableActionModel> MenuItems          { get; }

        public SelectableActionModel                     SelectedMenuItem   => MenuItems?.FirstOrDefault(model => model.IsSelected);
        public string                                    Title              => SelectedMenuItem?.Name;

        public ShellViewModel() {
            MenuItems = new BindableCollection<SelectableActionModel> {
                new SelectableActionModel(
                    name: "History",
                    iconName: "IconHistory",
                    action: () => ActivateItem(ViewModelFactory.Create<HistoryViewModel>())
                ),
                new SelectableActionModel(
                    name: "Favorite",
                    iconName: "IconStarFull",
                    action: () => ActivateItem(ViewModelFactory.Create<FavoritesViewModel>())
                ),
                new SelectableActionModel(
                    name: "Search",
                    iconName: "IconSearch",
                    action: () => ActivateItem(ViewModelFactory.Create<SearchViewModel>())
                )
            };
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            Events.Subscribe(this);

            if (!MenuItems.Any(m => m.IsSelected)) {
                SelectMenuItem(MenuItems.First());
            }
        }

        public void SelectMenuItem(SelectableActionModel option) {
            if (option == SelectedMenuItem) {
                return;
            }

            foreach (SelectableActionModel barItemModel in MenuItems.Where(m => m.IsSelected)) {
                barItemModel.IsSelected = false;
            }

            ClipboardItemsProvider.SetFilter(null);
            option.IsSelected = true;
            option.Action?.Invoke();
            NotifyOfPropertyChange(() => SelectedMenuItem);
            NotifyOfPropertyChange(() => Title);
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
            ClipboardItemsProvider.SetFilter(message.Filter);
        }

        #endregion

    }

}
