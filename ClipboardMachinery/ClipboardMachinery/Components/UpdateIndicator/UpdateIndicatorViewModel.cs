using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using ClipboardMachinery.Plumbing.Factories;
using ClipboardMachinery.Windows.UpdateNotes;
using Onova;
using Onova.Models;
using Timer = System.Timers.Timer;

namespace ClipboardMachinery.Components.UpdateIndicator {

    public class UpdateIndicatorViewModel : Screen {

        #region Properties

        public string DisplayText {
            get => displayText;
            private set {
                if (displayText == value) {
                    return;
                }

                displayText = value;
                NotifyOfPropertyChange();
            }
        }

        public SolidColorBrush StatusColor {
            get => statusColor;
            private set {
                if (Equals(statusColor, value)) {
                    return;
                }

                statusColor = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsLoading {
            get => isLoading;
            private set {
                if (isLoading == value) {
                    return;
                }

                isLoading = value;
                NotifyOfPropertyChange();
            }
        }

        internal IndicatorState State {
            get => state;
            private set {
                if (state == value) {
                    return;
                }

                state = value;
                UpdateState();
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => CanHandleInteraction);
                NotifyOfPropertyChange(() => InteractionType);
            }
        }

        public bool CanHandleInteraction =>
            State == IndicatorState.UP_TO_DATE ||
            State == IndicatorState.UPDATE_AVAILABLE ||
            State == IndicatorState.UPDATE_READY;

        public Cursor InteractionType
            => CanHandleInteraction ? Cursors.Hand : Cursors.Arrow;

        #endregion

        #region Fields

        private static readonly SolidColorBrush standbyColor = new SolidColorBrush(Colors.Gray);
        private static readonly SolidColorBrush workingColor = Application.Current.FindResource("PanelControlBrush") as SolidColorBrush;
        private static readonly SolidColorBrush updateAvailableColor = Application.Current.FindResource("PositiveActionBrush") as SolidColorBrush;
        private static readonly SolidColorBrush updateReadyColor = Application.Current.FindResource("ElementFavoriteBrush") as SolidColorBrush;

        private readonly UpdateManager updateManager;
        private readonly IWindowManager windowManager;
        private readonly IWindowFactory windowFactory;
        private readonly Timer refresh;

        private string displayText;
        private SolidColorBrush statusColor = standbyColor;
        private bool isLoading;
        private IndicatorState state;
        private CheckForUpdatesResult lastUpdateCheckResult;

        #endregion

        public UpdateIndicatorViewModel(UpdateManager updateManager, IWindowManager windowManager, IWindowFactory windowFactory) {
            this.updateManager = updateManager;
            this.windowManager = windowManager;
            this.windowFactory = windowFactory;

            // Setup update check timer
            refresh = new Timer {
                Interval = 1000 * 60 * 60, // TODO: Make interval configurable
                AutoReset = true,
                Enabled = false
            };

            refresh.Elapsed += OnRefreshTimerTick;
        }

        #region Logic

        public async Task CheckForUpdates() {
            // Check for new version
            State = IndicatorState.REFRESH;
            lastUpdateCheckResult = await updateManager.CheckForUpdatesAsync();
            await Task.Delay(1500);

            // Do not allow updater to run in local development versions
            /*
            if (App.CurrentVersion == App.DevelopmentVersion) {
                State = IndicatorState.UP_TO_DATE;
                DisplayText = "Updater disabled in development version.";
                return;
            }
            */

            // Check if there is a update package that is ready to be installed
            if (updateManager.IsUpdatePrepared(lastUpdateCheckResult.LastVersion)) {
                State = IndicatorState.UPDATE_READY;
                return;
            }

            // Update state according to result
            State = lastUpdateCheckResult.CanUpdate
                ? IndicatorState.UPDATE_AVAILABLE
                : IndicatorState.UP_TO_DATE;
        }

        private void UpdateState() {
            switch (State) {
                case IndicatorState.UNKNOWN:
                    StatusColor = standbyColor;
                    DisplayText = string.Empty;
                    IsLoading = false;
                    break;

                case IndicatorState.UP_TO_DATE:
                    StatusColor = standbyColor;
                    DisplayText = "Up to date.";
                    IsLoading = false;
                    break;

                case IndicatorState.REFRESH:
                    StatusColor = workingColor;
                    DisplayText = "Checking for updates...";
                    IsLoading = true;
                    break;

                case IndicatorState.UPDATE_AVAILABLE:
                    StatusColor = updateAvailableColor;
                    DisplayText = $"New version {lastUpdateCheckResult?.LastVersion} is available!";
                    IsLoading = false;
                    break;

                case IndicatorState.UPDATE_AWAITING_CONFIRMATION:
                    StatusColor = standbyColor;
                    DisplayText = "Awaiting confirmation...";
                    IsLoading = true;
                    break;

                case IndicatorState.UPDATE_DOWNLOAD:
                    StatusColor = workingColor;
                    DisplayText = "Downloading update...";
                    IsLoading = true;
                    break;

                case IndicatorState.UPDATE_READY:
                    StatusColor = updateReadyColor;
                    DisplayText = "Update is ready to be installed! Click here to restart.";
                    IsLoading = false;
                    break;
            }
        }

        #endregion

        #region Handlers

        protected override Task OnActivateAsync(CancellationToken cancellationToken) {
            if (refresh != null) {
                refresh.Enabled = true;
            }

            // Update indicator with initial state
            State = IndicatorState.UP_TO_DATE;

            // Initial check for updates
            Task.Run(CheckForUpdates, cancellationToken);

            return base.OnActivateAsync(cancellationToken);
        }

        private async void OnRefreshTimerTick(object sender, ElapsedEventArgs e) {
            await CheckForUpdates();
        }

        private void OnDownloadProgressProgressChanged(object sender, double progress) {
            DisplayText = $"Downloading update... ({Math.Round(progress * 100)}%)";
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            refresh.Elapsed -= OnRefreshTimerTick;
            refresh.Dispose();
            return Task.CompletedTask;
        }

        #endregion

        #region Actions

        public async Task HandleInteraction() {
            // Bail out if interaction is disabled
            if (!CanHandleInteraction) {
                return;
            }

            // Handle click interaction
            switch (State) {
                case IndicatorState.UP_TO_DATE:
                    await CheckForUpdates();
                    break;

                case IndicatorState.UPDATE_AVAILABLE:
                    State = IndicatorState.UPDATE_AWAITING_CONFIRMATION;
                    UpdateNotesViewModel updateNotes = windowFactory.CreateUpdateNotesWindow(lastUpdateCheckResult.LastVersion);

                    if (await windowManager.ShowDialogAsync(updateNotes) == true) {
                        State = IndicatorState.UPDATE_DOWNLOAD;
                        Progress<double> downloadProgress = new Progress<double>();
                        downloadProgress.ProgressChanged += OnDownloadProgressProgressChanged;
                        await Task.Run(() =>
                            updateManager.PrepareUpdateAsync(lastUpdateCheckResult.LastVersion, downloadProgress)
                        );
                        downloadProgress.ProgressChanged -= OnDownloadProgressProgressChanged;
                    }

                    windowFactory.Release(updateNotes);
                    await CheckForUpdates();
                    break;

                case IndicatorState.UPDATE_READY:
                    updateManager.LaunchUpdater(lastUpdateCheckResult.LastVersion);
                    Application.Current.Shutdown();
                    break;
            }
        }

        #endregion

    }

}
