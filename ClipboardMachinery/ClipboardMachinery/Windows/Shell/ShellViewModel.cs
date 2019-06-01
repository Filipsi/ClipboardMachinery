using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Core.Data;
using ClipboardMachinery.Core.Services.Clipboard;
using ClipboardMachinery.Core.Services.HotKeys;
using ClipboardMachinery.Plumbing;
using ClipboardMachinery.Popup.Manager;
using static ClipboardMachinery.Common.Events.ClipEvent;

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
            => (Debugger.IsAttached ? "dev" : string.Empty) + Assembly.GetEntryAssembly()?.GetName().Version.ToString(3);

        public double AppWidth
            => SystemParameters.PrimaryScreenWidth / 3;

        public double MaxContentHeight
            => SystemParameters.PrimaryScreenHeight / 1.5;

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IDataRepository dataRepository;
        private readonly IClipboardService clipboardService;

        private bool isVisible = true;
        private string lastAcceptedClipContent;

        #endregion

        public ShellViewModel(
            IEventAggregator eventAggregator, NavigatorViewModel navigator, PopupManagerViewModel popupWrapperVm,
            IHotKeyService hotKeyService, IClipboardService clipboardService, IDataRepository dataRepository)  {

            this.eventAggregator = eventAggregator;
            this.clipboardService = clipboardService;

            // Data repository
            this.dataRepository = dataRepository;
            lastAcceptedClipContent = dataRepository.LastClipContent;

            // Popup wrapper
            Popup = popupWrapperVm;
            Popup.ConductWith(this);

            // HotKeys
            hotKeyService.Register(System.Windows.Input.Key.H, KeyModifier.Ctrl, OnAppVisiblityToggle);

            // Clipboard
            clipboardService.ClipboardChanged += OnClipboardChanged;

            // Navigator
            Navigator = navigator;
            Navigator.ConductWith(this);
            Navigator.ExitButtonClicked += OnNavigatorExitButtonClicked;
            Navigator.PropertyChanged += OnNavigatorPropertyChanged;
        }

        #region Handlers

        private void OnAppVisiblityToggle(HotKey key) {
            IsVisible = !IsVisible;
        }

        public Task HandleAsync(ClipEvent message, CancellationToken cancellationToken) {
            if (message.EventType != ClipEventType.Select) {
                return Task.CompletedTask;
            }

            clipboardService.IgnoreNextChange(message.Source.Content);
            clipboardService.SetClipboardContent(message.Source.Content);
            IsVisible = false;
            return Task.CompletedTask;
        }

        private async void OnNavigatorExitButtonClicked(object sender, EventArgs e) {
            await TryCloseAsync();
        }

        private async void OnNavigatorPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != nameof(Navigator.Selected)) {
                return;
            }

            if (sender is NavigatorViewModel navigator) {
                await ActivateItemAsync(navigator.Selected, CancellationToken.None);
            }
        }

        private void OnClipboardChanged(object sender, ClipboardEventArgs e) {
            // Check if there is anything to save
            if (string.IsNullOrWhiteSpace(e.Payload)) {
                return;
            }

            // Prevent from saving duplicates right after each other
            // NOTE: This was made specifically for trigger happy @Jemmotar
            if (e.Payload == lastAcceptedClipContent) {
                return;
            }

            // Accept new clip from clipboard change
            Task.Run(() => AcceptClip(e.Payload, e.Source));
        }

        #endregion

        #region Logic

        private async Task AcceptClip(string content, string source) {
            // Update last accepted content
            lastAcceptedClipContent = content;

            // Save clip
            ClipModel model = await dataRepository.CreateClip<ClipModel>(
                content: content,
                created: DateTime.UtcNow,
                tags: new[] {
                    // TODO: Add config option to disable this
                    new KeyValuePair<string, object>("source", source)
                }
            );

            // Dispatch information about new clip creation
            await eventAggregator.PublishOnUIThreadAsync(
                new ClipEvent(
                    model,
                    ClipEventType.Created
                )
            );
        }

        #endregion

    }

}
