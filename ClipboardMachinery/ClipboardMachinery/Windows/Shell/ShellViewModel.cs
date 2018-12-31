using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Caliburn.Micro;
using Castle.Windsor;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Core.Repository;
using ClipboardMachinery.Core.Services.Clipboard;
using ClipboardMachinery.Core.Services.HotKeys;
using ClipboardMachinery.Plumbing;
using ClipboardMachinery.Plumbing.Factories;
using ClipboardMachinery.Popup.Manager;
using ClipboardMachinery.Popup.TagEditor;
using static ClipboardMachinery.Common.Events.ClipEvent;
using static ClipboardMachinery.Common.Events.PopupEvent;

namespace ClipboardMachinery.Windows.Shell {

    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell, IHandle<ClipEvent> {

        #region Properties

        public bool IsVisible {
            get => isVisible;
            set {
                if (isVisible == value) {
                    return;
                }

                isVisible = value;
                NotifyOfPropertyChange();
            }
        }

        public NavigatorViewModel Navigator {
            get;
        }

        public PopupManagerViewModel Popup {
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

        private readonly IEventAggregator eventAggregator;
        private readonly IWindsorContainer windsorContainer;
        private readonly IDataRepository dataRepository;
        private readonly IClipboardService clipboardService;

        private bool isVisible = true;

        #endregion

        public ShellViewModel(
            IEventAggregator eventAggregator, NavigatorViewModel navigator, PopupManagerViewModel popupWrapperVm,
            IWindsorContainer windsorContainer, IHotKeyService hotKeyService,
            IClipboardService clipboardService, IDataRepository dataRepository)  {

            this.eventAggregator = eventAggregator;
            this.windsorContainer = windsorContainer;
            this.dataRepository = dataRepository;
            this.clipboardService = clipboardService;

            // Popup wrapper
            Popup = popupWrapperVm;
            Popup.ConductWith(this);
            Popup.DeactivateWith(this);

            // HotKeys
            hotKeyService.Register(System.Windows.Input.Key.H, KeyModifier.Ctrl, OnAppVisiblityToggle);

            // Clipboard
            clipboardService.ClipboardChanged += OnClipboardChanged;

            // Navigator
            Navigator = navigator;
            Navigator.ConductWith(this);
            Navigator.DeactivateWith(this);
            Navigator.ExitButtonClicked += OnNavigatorExitButtonClicked;
            Navigator.PropertyChanged += OnNavigatorPropertyChanged;
        }

        #region Event Handlers

        private void OnAppVisiblityToggle(HotKey key) {
            IsVisible = !IsVisible;
        }

        public Task HandleAsync(ClipEvent message, CancellationToken cancellationToken) {
            if (message.EventType == ClipEventType.Select) {
                clipboardService.IgnoreNextChange(message.Source.Content);
                clipboardService.SetClipboardContent(message.Source.Content);
                IsVisible = false;
            }

            return Task.CompletedTask;
        }

        private void OnNavigatorExitButtonClicked(object sender, EventArgs e) {
            TryClose();
        }

        private void OnNavigatorPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != nameof(Navigator.Selected)) {
                return;
            }

            NavigatorViewModel navigator = sender as NavigatorViewModel;

            if (navigator.Selected == null) {
                ActivateItem(null);

            } else {
                navigator.Selected.ConductWith(this);
                ActivateItem(navigator.Selected as IScreen);
            }
        }

        private async void OnClipboardChanged(object sender, ClipboardEventArgs e) {
            if (e.Payload == string.Empty) {
                return;
            }

            // Save clip
            ClipModel model = await dataRepository.InsertClip<ClipModel>(
                content: e.Payload,
                created: DateTime.UtcNow,
                tags: new KeyValuePair<string, object>[] {
                    // TODO: Add config option to disable this
                    new KeyValuePair<string, object>("source", e.Source)
                }
            );

            // Dispatch information about new clip creation
            await eventAggregator.PublishOnCurrentThreadAsync(
                new ClipEvent(
                    model,
                    ClipEventType.Created
                )
            );
        }

        #endregion

    }

}
