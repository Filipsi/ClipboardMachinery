using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.ActionButton;
using ClipboardMachinery.Popup.Manager.Interfaces;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static ClipboardMachinery.Common.Events.PopupEvent;

namespace ClipboardMachinery.Popup.Manager {

    public class PopupManagerViewModel : Screen, IHandle<PopupEvent> {

        #region Properties

        public BindableCollection<ActionButtonViewModel> Controls {
            get;
        }

        public IScreen PopupContent {
            get => popupContent;
            private set {
                if (popupContent == value) {
                    return;
                }

                if (popupContent is IExtensionControlsProvider oldControls) {
                    oldControls.ExtensionControls.CollectionChanged -= OnExtensionControlsCollectionChanged;
                    OnExtensionControlsCollectionChanged(
                        sender: oldControls.ExtensionControls,
                        e: new NotifyCollectionChangedEventArgs(
                            action: NotifyCollectionChangedAction.Reset
                        )
                    );
                }

                if (value is IExtensionControlsProvider newControls) {
                    newControls.ExtensionControls.CollectionChanged += OnExtensionControlsCollectionChanged;
                    OnExtensionControlsCollectionChanged(
                        sender: newControls.ExtensionControls,
                        e: new NotifyCollectionChangedEventArgs(
                            action: NotifyCollectionChangedAction.Add,
                            changedItems: newControls.ExtensionControls.ToArray()
                        )
                    );
                }

                popupContent = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => HasPopupOverlay);
            }
        }



        public bool HasPopupOverlay
            => popupContent != null;

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IScreen popupContent;

        #endregion

        public PopupManagerViewModel(IEventAggregator eventAggregator, Func<ActionButtonViewModel> actionButtonFactory) {
            this.eventAggregator = eventAggregator;

            Controls = new BindableCollection<ActionButtonViewModel>();

            // Create control buttons
            ActionButtonViewModel button = actionButtonFactory.Invoke();
            button.ToolTip = "Close";
            button.Icon = (Geometry)Application.Current.FindResource("IconExit");
            button.ClickAction = HandleCloseClick;
            Controls.Add(button);
        }

        protected override void OnDeactivate(bool close) {
            base.OnDeactivate(close);

            if (close) {
                // Unhook events when closing
                PopupContent = null;
            }
        }

        #region Handlers

        private void OnExtensionControlsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    Controls.AddRange(e.NewItems.Cast<ActionButtonViewModel>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    Controls.RemoveRange((e.OldItems ?? sender as IList).Cast<ActionButtonViewModel>());
                    break;
            }
        }

        private void HandleCloseClick(ActionButtonViewModel button) {
            eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Close());
        }

        public Task HandleAsync(PopupEvent message, CancellationToken cancellationToken) {
            switch (message.EventType) {
                case PopupEventType.Show:
                    PopupContent = message.Popup;
                    PopupContent.ConductWith(this);
                    PopupContent.Activate();
                    break;

                case PopupEventType.Close:
                    PopupContent.Deactivate(true);
                    PopupContent = null;
                    break;
            }

            return Task.CompletedTask;
        }

        #endregion

    }
}
