using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Castle.Windsor;
using ClipboardMachinery.Components.ActionButton;

namespace ClipboardMachinery.Components.Navigator {

    public class NavigatorViewModel : Screen {

        #region Properties

        public BindableCollection<ActionButtonViewModel> Pages {
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

        public NavigatorViewModel(IWindsorContainer container, Func<ActionButtonViewModel> buttonVmFactory) {

            // Automatically create pages from ViewModels that implements IScreenPage
            List<IScreenPage> pages = container.ResolveAll<IScreenPage>().ToList();
            pages.Sort((x, y) => x.Order.CompareTo(y.Order));

            Pages = new BindableCollection<ActionButtonViewModel>(
                pages.Select(page => {
                    ActionButtonViewModel button = buttonVmFactory.Invoke();
                    button.CanBeSelected = true;
                    button.ToolTip = page.Title;
                    button.Icon = (Geometry)Application.Current.FindResource(page.Icon);
                    button.Model = page;
                    button.ClickAction = HandleNavigationClick;
                    return button;
                })
            );

            // Create controls
            Controls = new BindableCollection<ActionButtonViewModel>();

            ActionButtonViewModel removeButton = buttonVmFactory.Invoke();
            removeButton.Icon = (Geometry)Application.Current.FindResource("IconExit");
            removeButton.ClickAction = Exit;
            Controls.Add(removeButton);
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            // Select first page if no page is selected
            if (Pages.Count > 0 && Selected == null) {
                HandleNavigationClick(Pages.First());
            }
        }

        #region Handlers

        private void HandleNavigationClick(ActionButtonViewModel control) {
            // De-select currently selected pages
            foreach (ActionButtonViewModel pageControl in Pages) {
                pageControl.IsSelected = false;
            }

            // Select new one and notify about changes
            control.IsSelected = true;
            NotifyOfPropertyChange(() => Selected);
            NotifyOfPropertyChange(() => SelectedPageTitle);
        }

        private void Exit(object arg)
            => ExitButtonClicked?.Invoke(this, EventArgs.Empty);

        #endregion

    }

}
