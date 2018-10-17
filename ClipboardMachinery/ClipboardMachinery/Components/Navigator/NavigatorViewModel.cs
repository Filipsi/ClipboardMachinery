using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Castle.Windsor;
using ClipboardMachinery.Common.Models;
using ClipboardMachinery.Components.ActionButton;
using ClipboardMachinery.Plumbing;

namespace ClipboardMachinery.Components.Navigator {

    public class NavigatorViewModel : Screen {

        #region Properties

        public BindableCollection<NavigatorModel> Pages {
            get;
        }

        public BindableCollection<ActionButtonViewModel> Controls {
            get;
        }

        public NavigatorModel Selected
            => Pages?.FirstOrDefault(model => model.IsSelected);

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

            Pages = new BindableCollection<NavigatorModel>(
               pages.Select(page => new NavigatorModel(page))
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
                SelectPage(Pages.First());
            }
        }

        #region Handlers

        public void SelectPage(NavigatorModel page) {
            // Ignore currently selected page (can't be selected again)
            if (page == Selected) {
                return;
            }

            // De-select currently selected pages
            foreach (NavigatorModel pageModel in Pages.Where(m => m.IsSelected)) {
                pageModel.IsSelected = false;
            }

            // Select new one and notify about changes
            page.IsSelected = true;
            NotifyOfPropertyChange(() => Selected);
            NotifyOfPropertyChange(() => SelectedPageTitle);
        }

        private void Exit()
            => ExitButtonClicked?.Invoke(this, EventArgs.Empty);

        #endregion

    }

}
