using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClipboardMachinery.Events;
using ClipboardMachinery.Models;

namespace ClipboardMachinery.ViewModels {

    public class HorizontalMenuViewModel : Screen {

        #region Properties

        public BindableCollection<PageNavigatorModel> Pages {
            get => pages;
            set {
                if (pages == value)
                    return;

                pages = value;
                NotifyOfPropertyChange(() => Pages);
                NotifyOfPropertyChange(() => SelectedPage);
            }
        }

        public BindableCollection<ActionButtonModel> Controls {
            get => controls;
            set {
                if (controls == value)
                    return;

                controls = value;
                NotifyOfPropertyChange(() => Controls);
            }
        }

        public PageNavigatorModel SelectedPage
            => Pages?.FirstOrDefault(model => model.IsSelected);

        public string SelectedPageTitle
            => SelectedPage?.Name;

        #endregion

        #region Fields

        private BindableCollection<PageNavigatorModel> pages;
        private BindableCollection<ActionButtonModel> controls;
        private readonly IEventAggregator eventBus;

        #endregion

        public HorizontalMenuViewModel(IEventAggregator eventAggregator) {
            eventBus = eventAggregator;
        }

        #region Handlers

        protected override void OnInitialize() {
            if (!Pages.Any(m => m.IsSelected)) {
                SelectPage(Pages.First());
            }

            base.OnInitialize();
        }

        public void SelectPage(PageNavigatorModel page) {
            if (page == SelectedPage) {
                return;
            }

            foreach (PageNavigatorModel barItemModel in Pages.Where(m => m.IsSelected)) {
                barItemModel.IsSelected = false;
            }

            page.IsSelected = true;
            NotifyOfPropertyChange(() => SelectedPage);
            NotifyOfPropertyChange(() => SelectedPageTitle);
        }

        public void HandleControlClick(ActionButtonModel control)
            => control?.InvokeClickAction();

        #endregion

    }

}
