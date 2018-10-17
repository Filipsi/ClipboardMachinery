using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using Castle.Windsor;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Core.Services.Clipboard;
using ClipboardMachinery.Core.Services.HotKeys;

namespace ClipboardMachinery.Components.Shell {

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

        public NavigatorViewModel Navigator {
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

        private bool isVisible = true;

        #endregion

        public ShellViewModel(
            NavigatorViewModel navigator,
            IWindsorContainer windsorContainer, IHotKeyService hotKeyService, IClipboardService clipboardService,
            Func<ClipViewModel> clipViewModelFactory)  {

            container = windsorContainer;
            clipVmFactory = clipViewModelFactory;

            // HotKeys
            hotKeyService.Register(System.Windows.Input.Key.H, KeyModifier.Ctrl, OnAppVisiblityToggle);

            // Clipboard
            ClipboardItems = new BindableCollection<ClipViewModel>();
            ClipboardItemsView = CollectionViewSource.GetDefaultView(ClipboardItems);
            clipboardService.ClipboardChanged += OnClipboardChanged;

            // Navigator
            Navigator = navigator;
            Navigator.ConductWith(this);
            Navigator.ExitButtonClicked += OnNavigatorExitButtonClicked;
            Navigator.PropertyChanged += OnNavigatorPropertyChanged;
        }

        #region IShell

        public void SetClipViewFiler(Predicate<object> filter)
            => ClipboardItemsView.Filter = filter;

        #endregion

        #region Event Handlers

        private void OnAppVisiblityToggle(HotKey key) {
            IsVisible = !IsVisible;
        }

        private void OnNavigatorExitButtonClicked(object sender, EventArgs e) {
            TryClose();
        }

        private void OnNavigatorPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != nameof(Navigator.Selected)) {
                return;
            }

            NavigatorViewModel navigator = sender as NavigatorViewModel;
            SetClipViewFiler(null);

            if(navigator.Selected == null) {
                ActivateItem(null);
            } else {
                navigator.Selected.ConductWith(this);
                ActivateItem(navigator.Selected as IScreen);
            }
        }

        private void OnClipboardChanged(object sender, ClipboardEventArgs e) {
            if (e.Payload == string.Empty || e.Payload == ClipboardItems.FirstOrDefault()?.Model.Content) {
                return;
            }

            ClipViewModel vm = clipVmFactory.Invoke();
            vm.Model = new ClipModel {
                Created = DateTime.UtcNow,
                Content = e.Payload
            };

            ClipboardItems.Insert(0, vm);
        }

        #endregion

    }

}
