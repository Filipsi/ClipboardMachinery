using System.Collections;
using System.Linq;
using Caliburn.Micro;
using Castle.Windsor;
using ClipboardMachinery.Models;
using ClipboardMachinery.Plumbing;

namespace ClipboardMachinery.ViewModels {

    public class NavigatorViewModel : Screen {

        #region Properties

        public BindableCollection<PageButtonModel> Pages {
            get;
        }

        public BindableCollection<ActionButtonModel> Controls {
            get;
        }

        public PageButtonModel SelectedPage
            => Pages?.FirstOrDefault(model => model.IsSelected);

        public string SelectedPageTitle
            => SelectedPage?.Title;

        #endregion

        public NavigatorViewModel(IWindsorContainer container) {
            // Automatically create pages from ViewModels that implement IPage
            Pages = new BindableCollection<PageButtonModel>(
                container.ResolveAll<IPage>().Select(page => new PageButtonModel(page))
            );

            // Select first page if no page is selected
            if (Pages.Count > 0 && SelectedPage == null) {
                SelectPage(Pages.First());
            }

            // Create control buttons
            Controls = new BindableCollection<ActionButtonModel> {
                new ActionButtonModel(
                    iconName: "IconExit",
                    clickAction: () => TryClose() // TODO
                )
            };
        }

        #region Handlers

        public void SelectPage(PageButtonModel page) {
            // Ignore currently selected page (can't be selected again)
            if (page == SelectedPage) {
                return;
            }

            // De-select currently selected pages
            foreach (PageButtonModel pageModel in Pages.Where(m => m.IsSelected)) {
                pageModel.IsSelected = false;
            }

            // Select new one and notify about changes
            page.IsSelected = true;
            NotifyOfPropertyChange(() => SelectedPage);
            NotifyOfPropertyChange(() => SelectedPageTitle);
        }

        public void HandleControlClick(ActionButtonModel control)
            => control?.InvokeClickAction();

        #endregion

    }

}
