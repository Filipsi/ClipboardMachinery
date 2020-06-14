using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Caliburn.Micro;
using Castle.Core.Logging;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.ContentPresenter;
using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Components.UpdateIndicator;
using ClipboardMachinery.Core;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Core.Services.Clipboard;
using ClipboardMachinery.Core.Services.HotKeys;
using ClipboardMachinery.Plumbing;
using static ClipboardMachinery.Common.Events.ClipEvent;

namespace ClipboardMachinery.Windows.Shell {

    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell, IHandle<ClipEvent> {

        #region Properties

        public ILogger Logger {
            get;
            set;
        }

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

        public IScreen DialogOverlayPortal {
            get;
        }

        public UpdateIndicatorViewModel UpdateIndicator {
            get;
        }

        public string AppVersion
            => App.CurrentVersion == App.DevelopmentVersion ? "DEVELOPMENT" : App.CurrentVersion.ToString(3);

        public double AppWidth
            => SystemParameters.PrimaryScreenWidth / 3;

        public double MaxContentHeight
            => SystemParameters.PrimaryScreenHeight / 1.5;

        public bool IsVisibleInTaskbar
            => Debugger.IsAttached;

        public bool IsTopmost
            => !Debugger.IsAttached;

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IDataRepository dataRepository;
        private readonly IClipboardService clipboardService;
        private readonly IContentDisplayResolver contentDisplayResolver;
        private readonly HotKey visiblitiyKeyBind;

        private bool isVisible = true;
        private string lastAcceptedClipContent;

        #endregion

        public ShellViewModel(
            IEventAggregator eventAggregator, NavigatorViewModel navigator, UpdateIndicatorViewModel updateIndicator,
            IDialogOverlayManager dialogOverlayManager, IHotKeyService hotKeyService, IClipboardService clipboardService,
            IDataRepository dataRepository, IContentDisplayResolver contentDisplayResolver)  {

            this.eventAggregator = eventAggregator;
            this.clipboardService = clipboardService;
            this.contentDisplayResolver = contentDisplayResolver;
            Logger = NullLogger.Instance;

            // Data repository
            this.dataRepository = dataRepository;
            lastAcceptedClipContent = dataRepository.LastClipContent;

            // Portal wrapper
            DialogOverlayPortal = dialogOverlayManager.Portal;
            DialogOverlayPortal.ConductWith(this);

            // Update indicator
            UpdateIndicator = updateIndicator;
            UpdateIndicator.ConductWith(this);

            // HotKeys
            visiblitiyKeyBind = hotKeyService.Register(Key.H, KeyModifier.Ctrl, OnAppVisiblityToggle);

            // Clipboard
            clipboardService.ClipboardChanged += OnClipboardChanged;

            // Navigator
            Navigator = navigator;
            Navigator.ConductWith(this);
            Navigator.ExitButtonClicked += OnNavigatorExitButtonClicked;
            Navigator.PropertyChanged += OnNavigatorPropertyChanged;
        }

        #region Handlers

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            visiblitiyKeyBind.Unregister();
            clipboardService.ClipboardChanged -= OnClipboardChanged;
            Navigator.ExitButtonClicked -= OnNavigatorExitButtonClicked;
            Navigator.PropertyChanged -= OnNavigatorPropertyChanged;
            return Task.CompletedTask;
        }

        public Task HandleAsync(ClipEvent message, CancellationToken cancellationToken) {
            if (message.EventType != ClipEventType.Select) {
                return Task.CompletedTask;
            }

            IsVisible = false;
            return Task.CompletedTask;
        }

        private void OnAppVisiblityToggle(HotKey key) {
            IsVisible = !IsVisible;
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

        private async void OnClipboardChanged(object sender, ClipboardEventArgs e) {
            // Check if there is anything to save
            if (string.IsNullOrWhiteSpace(e.Payload)) {
                return;
            }

            // Prevent from saving duplicates right after each other
            // NOTE: This was made specifically for trigger happy @Jemmotar
            if (e.Payload == lastAcceptedClipContent) {
                return;
            }

            // Update last accepted content
            lastAcceptedClipContent = e.Payload;

            // Accept new clip from clipboard change
            await Task.Run(() => AcceptClip(e.Payload, e.Source));
        }

        #endregion

        #region Logic

        private async Task AcceptClip(string content, string source) {
            // Resolve default presenter for the clip
            IContentPresenter defaultPresenter = contentDisplayResolver.GetDefaultPresenter(content);

            // Bail out if the app doesn't know how to display the clip
            if (defaultPresenter == null) {
                Logger.Warn("Unable to determinate content presenter, new clip won't be captured.");
                return;
            }

            Logger.Info($"Creating new clip from {source} with content presenter {defaultPresenter.Id} ({defaultPresenter.Name})...");

            // Save clip
            ClipModel model = await dataRepository.CreateClip<ClipModel>(
                content: content,
                contentPresenter: defaultPresenter.Id,
                tags: new[] {
                    // TODO: Add config option to disable this
                    new KeyValuePair<string, object>(SystemTagTypes.SourceTagType.Name, source),
                    new KeyValuePair<string, object>(SystemTagTypes.CreatedTagType.Name, DateTime.Now)
                }
            );

            Logger.Info($"Clip entry saved with Id {model.Id}.");

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
