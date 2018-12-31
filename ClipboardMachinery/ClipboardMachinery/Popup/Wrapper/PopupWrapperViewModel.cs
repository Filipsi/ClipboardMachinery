using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.ActionButton;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static ClipboardMachinery.Common.Events.PopupEvent;

namespace ClipboardMachinery.Popup.OverlayWrapper {

    public class PopupWrapperViewModel : Screen, IHandle<PopupEvent> {

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

        public PopupWrapperViewModel(IEventAggregator eventAggregator, Func<ActionButtonViewModel> actionButtonFactory) {
            this.eventAggregator = eventAggregator;

            Controls = new BindableCollection<ActionButtonViewModel>();

            // Create control buttons
            ActionButtonViewModel button = actionButtonFactory.Invoke();
            button.ToolTip = "Close";
            button.Icon = (Geometry)Application.Current.FindResource("IconExit");
            button.ClickAction = HandleCloseClick;
            Controls.Add(button);
        }

        #region Handlers

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
