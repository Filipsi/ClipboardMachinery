using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Buttons.ActionButton;

namespace ClipboardMachinery.Components.DialogOverlay.Impl.Portal {

    public class DialogOverlayPortalViewModel : Conductor<IScreen>, IHandle<DialogOverlayEvent> {

        #region Properties

        public BindableCollection<ActionButtonViewModel> Controls {
            get;
        }

        public bool HasActiveDialog
            => ActiveItem != null;

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;

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

        #region Handlers

        private async void OnExtensionControlsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
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

        private async Task HandleCloseClick(ActionButtonViewModel button) {
            await eventAggregator.PublishOnCurrentThreadAsync(DialogOverlayEvent.Close());
        }

        public async Task HandleAsync(DialogOverlayEvent message, CancellationToken cancellationToken) {
            switch (message.EventType) {
                case DialogOverlayEvent.PopupEventType.Open:
                    await ChangeActiveItemAsync(message.Popup, true, cancellationToken);
                    break;

                case DialogOverlayEvent.PopupEventType.Close:
                    await ChangeActiveItemAsync(null, true, cancellationToken);
                    break;
            }
        }

        protected override async Task ChangeActiveItemAsync(IScreen newItem, bool closePrevious, CancellationToken cancellationToken) {
            if (ActiveItem is IDialogOverlayControlsProvider oldControls) {
                oldControls.DialogControls.CollectionChanged -= OnExtensionControlsCollectionChanged;
                OnExtensionControlsCollectionChanged(
                    sender: oldControls.DialogControls,
                    e: new NotifyCollectionChangedEventArgs(
                        action: NotifyCollectionChangedAction.Reset
                    )
                );
            }

            await base.ChangeActiveItemAsync(newItem, closePrevious, cancellationToken);

            if (newItem is IDialogOverlayControlsProvider newControls) {
                newControls.DialogControls.CollectionChanged += OnExtensionControlsCollectionChanged;
                OnExtensionControlsCollectionChanged(
                    sender: newControls.DialogControls,
                    e: new NotifyCollectionChangedEventArgs(
                        action: NotifyCollectionChangedAction.Add,
                        changedItems: newControls.DialogControls.ToArray()
                    )
                );
            }

            NotifyOfPropertyChange(() => HasActiveDialog);
        }

        #endregion

    }
}
