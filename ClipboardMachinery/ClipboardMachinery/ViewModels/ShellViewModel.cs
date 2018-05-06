using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using ClipboardMachinery.Events;
using ClipboardMachinery.Logic;
using ClipboardMachinery.Logic.ClipboardItemsProvider;
using ClipboardMachinery.Logic.HotKeyHandler;
using ClipboardMachinery.Logic.ViewModelFactory;
using ClipboardMachinery.Models;
using Ninject;

namespace ClipboardMachinery.ViewModels {

    internal class ShellViewModel : Conductor<object>, IShell,
        IHandle<ChangeAppVisiblity>, IHandle<SetViewFilter>, IHandle<PageSelected> {

        [Inject]
        public IEventAggregator Events { set; get; }

        [Inject]
        public IViewModelFactory ViewModelFactory { set; get; }

        [Inject]
        public IClipboardItemsProvider ClipboardItemsProvider { set; get; }

        [Inject]
        public IHotKeyHandler HotKeyHandler { set; get; }

        public HorizontalMenuViewModel TopPanelMenu {
            get => _topPanelMenu;
            private set {
                if (Equals(value, _topPanelMenu)) return;
                _topPanelMenu = value;
                TopPanelMenu.ConductWith(this);
                NotifyOfPropertyChange(() => TopPanelMenu);
            }
        }

        public string AppVersion { get; } = (Debugger.IsAttached ? "dev" : string.Empty) +
                                            Assembly.GetEntryAssembly().GetName().Version.ToString(3);

        public double AppWidth { get; } = SystemParameters.PrimaryScreenWidth / 3;

        public double MaxContentHeight { get; } = SystemParameters.PrimaryScreenHeight / 1.5;

        private HorizontalMenuViewModel _topPanelMenu;

        protected override void OnInitialize() {
            base.OnInitialize();

            Events.Subscribe(this);
            TopPanelMenu = ViewModelFactory.Create<HorizontalMenuViewModel>();

            TopPanelMenu.Pages = new BindableCollection<PageNavigatorModel> {
                new PageNavigatorModel(
                    name: "History",
                    iconName: "IconHistory",
                    viewModelType: typeof(HistoryViewModel)
                ),
                new PageNavigatorModel(
                    name: "Favorite",
                    iconName: "IconStarFull",
                    viewModelType: typeof(FavoritesViewModel)
                ),
                new PageNavigatorModel(
                    name: "Search",
                    iconName: "IconSearch",
                    viewModelType: typeof(SearchViewModel)
                ),
                new PageNavigatorModel(
                    name: "Updater",
                    iconName: "IconUpdater",
                    viewModelType: typeof(UpdateViewModel)
                )
            };

            TopPanelMenu.Controls = new BindableCollection<ActionButtonModel> {
                new ActionButtonModel(
                    iconName: "IconExit",
                    clickAction: () => TryClose()
                )
            };
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
                        TopPanelMenu.SelectPage(TopPanelMenu.SelectedPage);
                        win.Show();
                    }
                    return;
            }
        }

        public void Handle(PageSelected message) {
            if (message.Source != TopPanelMenu) return;
            ClipboardItemsProvider.SetFilter(null);
            IScreen viewModel = ViewModelFactory.Create(message.Navigator.ViewModelType);
            viewModel.ConductWith(this);
            ActivateItem(viewModel);
        }

        public void Handle(SetViewFilter message) {
            ClipboardItemsProvider.SetFilter(message.Filter);
        }

        #endregion

    }

}
