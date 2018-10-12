using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using Castle.Windsor;
using ClipboardMachinery.Core.Services.Clipboard;
using ClipboardMachinery.Core.Services.HotKeys;
using ClipboardMachinery.Events;
using ClipboardMachinery.Models;
using ClipboardMachinery.Plumbing;

namespace ClipboardMachinery.ViewModels {

    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell {

        #region Properties

        public IObservableCollection<ClipViewModel> ClipboardItems {
            get;
        }

        public ICollectionView ClipboardItemsView {
            get;
        }

        public bool IsVisible {
            get => isVisible;
            set {
                if (isVisible == value)
                    return;

                isVisible = value;
                NotifyOfPropertyChange();
            }
        }

        public HorizontalMenuViewModel TopPanelMenu {
            get;
        }

        public string AppVersion
            => (Debugger.IsAttached ? "dev" : string.Empty) + Assembly.GetEntryAssembly().GetName().Version.ToString(3);

        public double AppWidth
            => SystemParameters.PrimaryScreenWidth / 3;

        public double MaxContentHeight
            => SystemParameters.PrimaryScreenHeight / 1.5;

        #endregion

        #region Fields

        private readonly IWindsorContainer container;
        private readonly Func<ClipViewModel> clipVmFactory;
        private readonly IEventAggregator eventBus;

        private bool isVisible = true;

        #endregion

        public ShellViewModel(
            HorizontalMenuViewModel topPanelMenu,
            IWindsorContainer windsorContainer, IEventAggregator eventAggregator, IHotKeyService hotKeyService,
            Func<ClipViewModel> clipViewModelFactory, IClipboardService clipboardService)  {

            container = windsorContainer;
            clipVmFactory = clipViewModelFactory;
            eventBus = eventAggregator;

            // HotKeys
            hotKeyService.Register(System.Windows.Input.Key.H, KeyModifier.Ctrl, OnAppVisiblityToggle);

            // Clipboard
            ClipboardItems = new BindableCollection<ClipViewModel>();
            ClipboardItemsView = CollectionViewSource.GetDefaultView(ClipboardItems);
            clipboardService.ClipboardChanged += OnClipboardChanged;

            // Navigator
            TopPanelMenu = topPanelMenu;
            TopPanelMenu.PropertyChanged += OnNavigatorPropertyChanged;
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            TopPanelMenu.Pages = new BindableCollection<PageNavigatorModel> {
                new PageNavigatorModel(
                    name: "History",
                    iconName: "IconHistory",
                    viewModelType: typeof(HistoryViewModel)
                ) {
                    IsSelected = true
                },
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

        #region IShell

        public void SetClipViewFiler(Predicate<object> filter)
            => ClipboardItemsView.Filter = filter;

        #endregion

        #region Event Handlers

        private void OnAppVisiblityToggle(HotKey key) {
            IsVisible = !IsVisible;

            /*
            if (IsVisible) {
                window.Hide();
            } else {
                TopPanelMenu.SelectPage(TopPanelMenu.SelectedPage);
                window.Show();
            }
            */
        }

        private void OnNavigatorPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != nameof(TopPanelMenu.SelectedPage)) {
                return;
            }

            HorizontalMenuViewModel navigator = sender as HorizontalMenuViewModel;
            SetClipViewFiler(null);

            if(navigator.SelectedPage == null) {
                ActivateItem(null);
            } else {
                IScreen viewModel = container.Resolve(navigator.SelectedPage.ViewModelType) as IScreen;
                viewModel.ConductWith(this);
                ActivateItem(viewModel);
            }
        }

        private void OnClipboardChanged(object sender, ClipboardEventArgs e) {
            if (e.Payload == string.Empty || e.Payload == ClipboardItems.FirstOrDefault()?.Model.RawContent) {
                return;
            }

            ClipViewModel vm = clipVmFactory.Invoke();
            vm.Model = new ClipModel {
                Created = DateTime.UtcNow,
                RawContent = e.Payload
            };

            ClipboardItems.Insert(0, vm);
        }

        public void Handle(ItemRemoved<ClipViewModel> message)
            => ClipboardItems.Remove(message.Item);

        #endregion

    }

}
