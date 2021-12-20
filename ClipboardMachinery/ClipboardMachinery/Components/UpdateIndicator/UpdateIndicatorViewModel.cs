using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using Castle.Core.Logging;
using ClipboardMachinery.Plumbing;
using ClipboardMachinery.Plumbing.Factories;
using ClipboardMachinery.Windows.UpdateNotes;
using Onova;
using Onova.Models;
using Timer = System.Timers.Timer;

namespace ClipboardMachinery.Components.UpdateIndicator {

    public class UpdateIndicatorViewModel : Screen {

        #region Properties

        public ILogger Logger { get; set; } = NullLogger.Instance;

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

        public IndicatorState State {
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

        public bool CanHandleInteraction {
            get => State == IndicatorState.UP_TO_DATE ||
                State == IndicatorState.REFRESH_FAILED ||
                State == IndicatorState.UPDATE_FAILED ||
                State == IndicatorState.UPDATE_AVAILABLE ||
                State == IndicatorState.UPDATE_READY;
        }

        public Cursor InteractionType {
            get => CanHandleInteraction ? Cursors.Hand : Cursors.Arrow;
        }

        #endregion

        #region Fields

        private static readonly SolidColorBrush standbyColor = new SolidColorBrush(Colors.Gray);
        private static readonly SolidColorBrush workingColor = Application.Current.FindResource("PanelControlBrush") as SolidColorBrush;
        private static readonly SolidColorBrush updateAvailableColor = Application.Current.FindResource("PositiveActionBrush") as SolidColorBrush;
        private static readonly SolidColorBrush updateReadyColor = Application.Current.FindResource("ElementFavoriteBrush") as SolidColorBrush;

        private readonly UpdateManager updateManager;
        private readonly LaunchOptions launchOptions;
        private readonly IWindowManager windowManager;
        private readonly IWindowFactory windowFactory;
        private readonly Timer refresh;

        private string displayText;
        private SolidColorBrush statusColor = standbyColor;
        private bool isLoading;
        private IndicatorState state;
        private CheckForUpdatesResult lastUpdateCheckResult;

        #endregion

        public UpdateIndicatorViewModel(UpdateManager updateManager, LaunchOptions launchOptions, IWindowManager windowManager, IWindowFactory windowFactory) {
            this.updateManager = updateManager;
            this.launchOptions = launchOptions;
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
            // Change state to render working indicator
            Logger.Debug("Checking for updates...");
            State = IndicatorState.REFRESH;
            await Task.Delay(1500);

            try {
                // Check for new version with within specified time
                Task<CheckForUpdatesResult> refreshTask = updateManager.CheckForUpdatesAsync();
                if (await Task.WhenAny(refreshTask, Task.Delay(1000 * 10)) != refreshTask) {
                    State = IndicatorState.REFRESH_FAILED;
                    Logger.Error("Failed to check for updates, task failed due to timeout!");
                    return;
                }

                // Store update check result
                lastUpdateCheckResult = refreshTask.Result;
            } catch (Exception ex) {
                Logger.Error("Failed to check for updates!", ex);
                State = IndicatorState.REFRESH_FAILED;
                return;
            }

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
                Logger.Info($"Update to version '{lastUpdateCheckResult.LastVersion}' is ready to be installed.");
                return;
            }

            if (App.CurrentVersion.CompareTo(lastUpdateCheckResult.LastVersion) != 0) {
                Logger.Info($"Found update to version '{lastUpdateCheckResult.LastVersion}'! Current application version is '{App.CurrentVersion.ToString(3)}'.");
                State = IndicatorState.UPDATE_AVAILABLE;
                return;
            }

            Logger.Debug("No updates found. application is up to date");
            State = IndicatorState.UP_TO_DATE;
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

                case IndicatorState.REFRESH_FAILED:
                    StatusColor = standbyColor;
                    DisplayText = "Unable to check for updates.";
                    IsLoading = false;
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

                case IndicatorState.UPDATE_FAILED:
                    StatusColor = standbyColor;
                    DisplayText = "Update failed.";
                    IsLoading = false;
                    break;

                case IndicatorState.UPDATE_READY:
                    StatusColor = updateReadyColor;
                    DisplayText = string.IsNullOrWhiteSpace(launchOptions.UpdaterBranch) ? "Update is ready to be installed! Click here to restart." : "Update is ready to be installed!";
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
                case IndicatorState.REFRESH_FAILED:
                case IndicatorState.UPDATE_FAILED:
                    await CheckForUpdates();
                    break;

                case IndicatorState.UPDATE_AVAILABLE:
                    State = IndicatorState.UPDATE_AWAITING_CONFIRMATION;

                    if (await ConfirmUpdateDownload(lastUpdateCheckResult.LastVersion)) {
                        // Change state to indicate start of update download
                        State = IndicatorState.UPDATE_DOWNLOAD;
                        Logger.Info($"Downloading update to version '{lastUpdateCheckResult.LastVersion}'...");

                        // Create a progress tracker
                        Progress<double> downloadProgress = new Progress<double>();
                        downloadProgress.ProgressChanged += OnDownloadProgressProgressChanged;

                        // Try to download the update
                        try {
                            await updateManager.PrepareUpdateAsync(lastUpdateCheckResult.LastVersion, downloadProgress);
                        } catch (Exception ex) {
                            State = IndicatorState.UPDATE_FAILED;
                            Logger.Error("Failed while downloading application update!", ex);
                            downloadProgress.ProgressChanged -= OnDownloadProgressProgressChanged;
                            break;
                        }

                        // Unhook progress handler from the tracker
                        downloadProgress.ProgressChanged -= OnDownloadProgressProgressChanged;
                    }

                    // Release the dialog and refresh updater state
                    Logger.Info("Update package successfully downloaded, refreshing...");

                    await CheckForUpdates();
                    break;

                case IndicatorState.UPDATE_READY:
                    Logger.Info("Restarting application by user request to update to newest version...");
                    updateManager.LaunchUpdater(lastUpdateCheckResult.LastVersion);
                    Application.Current.Shutdown();
                    break;
            }
        }

        private async Task<bool> ConfirmUpdateDownload(Version version) {
            if (!string.IsNullOrWhiteSpace(launchOptions.UpdaterBranch)) {
                return true;
            }

            UpdateNotesViewModel updateNotes = windowFactory.CreateUpdateNotesWindow(version);
            bool? result = await windowManager.ShowDialogAsync(updateNotes);
            windowFactory.Release(updateNotes);
            return result == true;
        }

        #endregion

    }

}
