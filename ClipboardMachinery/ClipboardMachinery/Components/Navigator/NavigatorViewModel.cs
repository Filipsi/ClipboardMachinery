using System;
using System.Linq;
using Caliburn.Micro;
using Castle.Windsor;
using ClipboardMachinery.Common.Models;
using ClipboardMachinery.Plumbing;

namespace ClipboardMachinery.Components.Navigator {

    public class NavigatorViewModel : Screen {

        #region Properties

        public BindableCollection<NavigatorModel> Pages {
            get;
        }

        public BindableCollection<ActionButtonModel> Controls {
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

        public NavigatorViewModel(IWindsorContainer container) {
            // Automatically create pages from ViewModels that implements IScreenPage
            Pages = new BindableCollection<NavigatorModel>(
                container.ResolveAll<IScreenPage>().Select(page => new NavigatorModel(page))
            );

            // Create control buttons
            Controls = new BindableCollection<ActionButtonModel> {
                new ActionButtonModel(
                    iconName: "IconExit",
                    clickAction: () => ExitButtonClicked?.Invoke(this, EventArgs.Empty)
                )
            };
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

        public void HandleControlClick(ActionButtonModel control)
            => control?.InvokeClickAction();

        #endregion

    }

}
