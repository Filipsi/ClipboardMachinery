using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Buttons.ActionButton;
using Action = System.Action;

namespace ClipboardMachinery.Components.DialogOverlay.Impl.Portal {

    public class DialogOverlayPortalViewModel : Conductor<IOverlayDialog> {

        #region Properties

        public BindableCollection<ActionButtonViewModel> Controls {
            get;
        }

        public bool HasActiveDialog
            => ActiveItem != null;

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private Action dialogReleaseFn;

        #endregion

        public DialogOverlayPortalViewModel(IEventAggregator eventAggregator, Func<ActionButtonViewModel> actionButtonFactory) {
            this.eventAggregator = eventAggregator;
            Controls = new BindableCollection<ActionButtonViewModel>();

            // Create control buttons
            ActionButtonViewModel button = actionButtonFactory.Invoke();
            button.ToolTip = "Close";
            button.Icon = (Geometry)Application.Current.FindResource("IconExit");
            button.ClickAction = HandleCloseClick;
            button.ConductWith(this);
            Controls.Add(button);
        }

        #region Logic

        public async Task OpenDialog(IOverlayDialog dialog) {
            await CloseDialog();
            await ChangeActiveItemAsync(dialog, true);
        }

        public async Task OpenDialog<T>(Func<T> createFn, Action<T> releaseFn) where T : IOverlayDialog {
            // Create new dialog with a abound release handle
            T newDialog = createFn.Invoke();
            dialogReleaseFn = () => releaseFn(newDialog);

            // Close previous dialog if there is any
            await CloseDialog();

            // Show new dialog
            await ChangeActiveItemAsync(newDialog, true);
        }

        public async Task CloseDialog() {
            if (ActiveItem == null) {
                return;
            }

            // Close current dialog
            await ChangeActiveItemAsync(null, true);

            // Release dialog if it is needed
            if (dialogReleaseFn != null) {
                dialogReleaseFn.Invoke();
                dialogReleaseFn = null;
            }
        }

        #endregion

        #region Handlers

        private Task HandleCloseClick(ActionButtonViewModel button) {
            return CloseDialog();
        }

        private async void OnDialogControlsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (ActionButtonViewModel extentionButton in e.NewItems) {
                        extentionButton.ConductWith(this);
                        Controls.Add(extentionButton);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (ActionButtonViewModel extentionButton in e.OldItems) {
                        await extentionButton.TryCloseAsync();
                        Controls.Remove(extentionButton);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (ActionButtonViewModel extentionButton in (IList<ActionButtonViewModel>) sender) {
                        await extentionButton.TryCloseAsync();
                        Controls.Remove(extentionButton);
                    }
                    break;
            }
        }

        private async void OnDialogPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IOverlayDialog.IsOpen):
                    if (!((IOverlayDialog) sender).IsOpen) {
                        await CloseDialog();
                    }
                    break;
            }
        }

        protected override async Task ChangeActiveItemAsync(IOverlayDialog newItem, bool closePrevious, CancellationToken cancellationToken) {
            // Deactivate old dialog if there is any
            if (ActiveItem != null) {
                ActiveItem.DialogControls.CollectionChanged -= OnDialogControlsCollectionChanged;
                OnDialogControlsCollectionChanged(
                    sender: ActiveItem.DialogControls,
                    e: new NotifyCollectionChangedEventArgs(
                        action: NotifyCollectionChangedAction.Reset
                    )
                );

                ActiveItem.PropertyChanged -= OnDialogPropertyChanged;
                ActiveItem.IsOpen = false;
                await eventAggregator.PublishOnCurrentThreadAsync(DialogOverlayEvent.CreateClosedEvent(ActiveItem), cancellationToken);
            }

            // Perform base logic
            await base.ChangeActiveItemAsync(newItem, closePrevious, cancellationToken);

            // Active new dialog if there is any
            if (newItem != null) {
                newItem.DialogControls.CollectionChanged += OnDialogControlsCollectionChanged;
                OnDialogControlsCollectionChanged(
                    sender: newItem.DialogControls,
                    e: new NotifyCollectionChangedEventArgs(
                        action: NotifyCollectionChangedAction.Add,
                        changedItems: newItem.DialogControls.ToArray()
                    )
                );

                ActiveItem.IsOpen = true;
                ActiveItem.AreControlsVisible = true;
                newItem.PropertyChanged += OnDialogPropertyChanged;
                await eventAggregator.PublishOnCurrentThreadAsync(DialogOverlayEvent.CreateOpenedEvent(ActiveItem), cancellationToken);
            }

            // Notify about change
            NotifyOfPropertyChange(() => HasActiveDialog);
        }

        #endregion

    }
}
