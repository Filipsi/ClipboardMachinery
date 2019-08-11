using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Castle.Windsor;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.Buttons.SelectableButton;
using static ClipboardMachinery.Common.Events.DialogOverlayEvent;

namespace ClipboardMachinery.Components.Navigator {

    public class NavigatorViewModel : Screen, IHandle<DialogOverlayEvent> {

        #region Properties

        public BindableCollection<SelectableButtonViewModel> Pages {
            get;
        }

        public BindableCollection<ActionButtonViewModel> Controls {
            get;
        }

        public IScreenPage Selected
            => Pages.FirstOrDefault(model => model.IsSelected)?.Model as IScreenPage;

        public string SelectedPageTitle
            => Selected?.Title;

        #endregion

        #region Events

        public event EventHandler ExitButtonClicked;

        #endregion

        public NavigatorViewModel(
            IWindsorContainer container, Func<ActionButtonViewModel> actionButtonFactory, Func<SelectableButtonViewModel> selectableButtonFactory) {

            // Automatically create pages from ViewModels that implements IScreenPage
            List<IScreenPage> pages = container.ResolveAll<IScreenPage>().ToList();
            pages.Sort((x, y) => x.Order.CompareTo(y.Order));

            Pages = new BindableCollection<SelectableButtonViewModel>(
                pages.Select(page => {
                    SelectableButtonViewModel button = selectableButtonFactory.Invoke();
                    button.ToolTip = page.Title;
                    button.Icon = (Geometry)Application.Current.FindResource(page.Icon);
                    button.Model = page;
                    button.ClickAction = HandleNavigationClick;
                    return button;
                })
            );

            // Create controls
            Controls = new BindableCollection<ActionButtonViewModel>();

            ActionButtonViewModel removeButton = actionButtonFactory.Invoke();
            removeButton.Icon = (Geometry)Application.Current.FindResource("IconExit");
            removeButton.ClickAction = Exit;
            Controls.Add(removeButton);
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken) {
            // Select first page if no page is selected
            if (Pages.Count > 0 && Selected == null) {
                HandleNavigationClick(Pages.First());
            }

            return base.OnInitializeAsync(cancellationToken);
        }

        #region Handlers

        private Task HandleNavigationClick(ActionButtonViewModel control) {
            if(!(control is SelectableButtonViewModel selectableControl)) {
                return Task.CompletedTask;
            }

            // De-select currently selected pages
            foreach (SelectableButtonViewModel pageControl in Pages) {
                pageControl.IsSelected = false;
            }

            // Select new one and notify about changes
            selectableControl.IsSelected = true;
            NotifyOfPropertyChange(() => Selected);
            NotifyOfPropertyChange(() => SelectedPageTitle);
            return Task.CompletedTask;
        }

        public Task HandleAsync(DialogOverlayEvent message, CancellationToken cancellationToken) {
            if (message.EventType != PopupEventType.Open && message.EventType != PopupEventType.Close) {
                return Task.CompletedTask;
            }

            foreach (SelectableButtonViewModel navigationButton in Pages) {
                navigationButton.IsEnabled = message.EventType == PopupEventType.Close;
            }

            return Task.CompletedTask;
        }

        private Task Exit(object arg) {
            ExitButtonClicked?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        #endregion

    }

}
