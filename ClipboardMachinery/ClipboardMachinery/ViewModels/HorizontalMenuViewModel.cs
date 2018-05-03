using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClipboardMachinery.Events;
using ClipboardMachinery.Models;
using Ninject;

namespace ClipboardMachinery.ViewModels {

    internal class HorizontalMenuViewModel : Screen {

        [Inject]
        public IEventAggregator Events { set; get; }

        public BindableCollection<PageNavigatorModel> Pages {
            get => _pages;
            set {
                if (Equals(value, _pages)) return;
                _pages = value;
                NotifyOfPropertyChange(() => Pages);
                NotifyOfPropertyChange(() => SelectedPage);
            }
        }

        public BindableCollection<ActionButtonModel> Controls {
            get => _controls;
            set {
                if (Equals(value, _controls)) return;
                _controls = value;
                NotifyOfPropertyChange(() => Controls);
            }
        }

        public PageNavigatorModel SelectedPage => Pages?.FirstOrDefault(model => model.IsSelected);
        public string SelectedPageTitle => SelectedPage?.Name;

        private BindableCollection<PageNavigatorModel> _pages;
        private BindableCollection<ActionButtonModel> _controls;

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
            Events.PublishOnUIThread(new PageSelected(this, page));
        }

        public void HandleControlClick(ActionButtonModel control)
            => control?.InvokeClickAction();
    }

}
