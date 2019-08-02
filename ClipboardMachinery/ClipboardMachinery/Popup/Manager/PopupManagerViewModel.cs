using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Buttons.ActionButton;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static ClipboardMachinery.Common.Events.PopupEvent;

namespace ClipboardMachinery.Popup.Manager {

    public class PopupManagerViewModel : Conductor<IScreen>, IHandle<PopupEvent> {

        #region Properties

        public BindableCollection<ActionButtonViewModel> Controls {
            get;
        }

        public bool HasPopupOverlay
            => ActiveItem != null;

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        public PopupManagerViewModel(IEventAggregator eventAggregator, Func<ActionButtonViewModel> actionButtonFactory) {
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
            await eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Close());
        }

        public async Task HandleAsync(PopupEvent message, CancellationToken cancellationToken) {
            switch (message.EventType) {
                case PopupEventType.Show:
                    await ChangeActiveItemAsync(message.Popup, true, cancellationToken);
                    break;

                case PopupEventType.Close:
                    await ChangeActiveItemAsync(null, true, cancellationToken);
                    break;
            }
        }

        protected override async Task ChangeActiveItemAsync(IScreen newItem, bool closePrevious, CancellationToken cancellationToken) {
            if (ActiveItem is IPopupExtendedControls oldControls) {
                oldControls.PopupControls.CollectionChanged -= OnExtensionControlsCollectionChanged;
                OnExtensionControlsCollectionChanged(
                    sender: oldControls.PopupControls,
                    e: new NotifyCollectionChangedEventArgs(
                        action: NotifyCollectionChangedAction.Reset
                    )
                );
            }

            await base.ChangeActiveItemAsync(newItem, closePrevious, cancellationToken);

            if (newItem is IPopupExtendedControls newControls) {
                newControls.PopupControls.CollectionChanged += OnExtensionControlsCollectionChanged;
                OnExtensionControlsCollectionChanged(
                    sender: newControls.PopupControls,
                    e: new NotifyCollectionChangedEventArgs(
                        action: NotifyCollectionChangedAction.Add,
                        changedItems: newControls.PopupControls.ToArray()
                    )
                );
            }

            NotifyOfPropertyChange(() => HasPopupOverlay);
        }

        #endregion

    }
}
