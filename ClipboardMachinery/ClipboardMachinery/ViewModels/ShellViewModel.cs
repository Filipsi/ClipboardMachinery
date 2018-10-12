using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Castle.Windsor;
using ClipboardMachinery.Events;
using ClipboardMachinery.Events.Collection;
using ClipboardMachinery.FileSystem;
using ClipboardMachinery.Logic;
using ClipboardMachinery.Logic.ClipboardItemsProvider;
using ClipboardMachinery.Logic.HotKeyHandler;
using ClipboardMachinery.Models;

namespace ClipboardMachinery.ViewModels {

    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell {

        #region Properties

        public IClipboardItemsProvider ClipboardItemsProvider {
            get;
        }

        public HorizontalMenuViewModel TopPanelMenu {
            get => topPanelMenu;
            private set {
                if (Equals(value, topPanelMenu))
                    return;

                topPanelMenu = value;
                TopPanelMenu.ConductWith(this);
                NotifyOfPropertyChange(() => TopPanelMenu);
            }
        }

        public string AppVersion
            => (Debugger.IsAttached ? "dev" : string.Empty) + Assembly.GetEntryAssembly().GetName().Version.ToString(3);

        public double AppWidth
            => SystemParameters.PrimaryScreenWidth / 3;

        public double MaxContentHeight
            => SystemParameters.PrimaryScreenHeight / 1.5;

        #endregion

        #region Fields

        private HorizontalMenuViewModel topPanelMenu;
        private readonly IWindsorContainer container;
        private readonly IHotKeyHandler hotKeys;

        #endregion

        public ShellViewModel(
            IClipboardItemsProvider clipboardItemsProvider, HorizontalMenuViewModel topPanelMenu,
            IWindsorContainer windsorContainer, IHotKeyHandler hotKeyHandler)  {

            ClipboardItemsProvider = clipboardItemsProvider;
            TopPanelMenu = topPanelMenu;
            container = windsorContainer;
            hotKeys = hotKeyHandler;
        }

        protected override void OnInitialize() {
            base.OnInitialize();

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

            switch (message.ChangeType) {
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
            if (message.Source != TopPanelMenu) {
                return;
            }

            ClipboardItemsProvider.SetFilter(null);

            // TODO: Create NavigationService
            IScreen viewModel = container.Resolve(message.Navigator.ViewModelType) as IScreen;
            viewModel.ConductWith(this);
            ActivateItem(viewModel);
        }

        #endregion

    }

}
