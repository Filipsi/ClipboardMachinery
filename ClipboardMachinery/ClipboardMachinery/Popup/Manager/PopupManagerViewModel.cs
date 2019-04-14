using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Popup.Manager.Interfaces;
using System;
using System.Collections;
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

        private void OnExtensionControlsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (ActionButtonViewModel extentionButton in e.NewItems) {
                        extentionButton.ConductWith(this);
                        Controls.Add(extentionButton);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (ActionButtonViewModel extentionButton in e.OldItems) {
                        extentionButton.TryClose();
                        Controls.Remove(extentionButton);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (ActionButtonViewModel extentionButton in sender as IList<ActionButtonViewModel>) {
                        extentionButton.TryClose();
                        Controls.Remove(extentionButton);
                    }
                    break;
            }
        }

        private void HandleCloseClick(ActionButtonViewModel button) {
            eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Close());
        }

        public Task HandleAsync(PopupEvent message, CancellationToken cancellationToken) {
            switch (message.EventType) {
                case PopupEventType.Show:
                    ChangeActiveItem(message.Popup, true);
                    break;

                case PopupEventType.Close:
                    ChangeActiveItem(null, true);
                    break;
            }

            return Task.CompletedTask;
        }

        protected override void ChangeActiveItem(IScreen newItem, bool closePrevious) {
            if (ActiveItem is IExtensionControlsProvider oldControls) {
                oldControls.ExtensionControls.CollectionChanged -= OnExtensionControlsCollectionChanged;
                OnExtensionControlsCollectionChanged(
                    sender: oldControls.ExtensionControls,
                    e: new NotifyCollectionChangedEventArgs(
                        action: NotifyCollectionChangedAction.Reset
                    )
                );
            }

            base.ChangeActiveItem(newItem, closePrevious);

            if (newItem is IExtensionControlsProvider newControls) {
                newControls.ExtensionControls.CollectionChanged += OnExtensionControlsCollectionChanged;
                OnExtensionControlsCollectionChanged(
                    sender: newControls.ExtensionControls,
                    e: new NotifyCollectionChangedEventArgs(
                        action: NotifyCollectionChangedAction.Add,
                        changedItems: newControls.ExtensionControls.ToArray()
                    )
                );
            }

            NotifyOfPropertyChange(() => HasPopupOverlay);
        }

        #endregion

    }
}
