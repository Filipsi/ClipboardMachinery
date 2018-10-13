using ClipboardMachinery.Common.Models;
using ClipboardMachinery.Plumbing;
using System.Windows;
using System.Windows.Media;

namespace ClipboardMachinery.Components.Navigator {

    public class NavigatorModel : ControlModel {

        #region Properties

        public IScreenPage Page {
            get;
        }

        public string Title
            => Page.Title;

        public new SolidColorBrush Color =>
            Application.Current.FindResource(IsSelected ? "PanelSelectedBrush" : "PanelControlBrush") as SolidColorBrush;

        public bool IsSelected {
            get => isSelected;
            set {
                if (value == isSelected)
                    return;

                isSelected = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => Color);
            }
        }

        #endregion

        #region Fields

        private bool isSelected;

        #endregion

        public NavigatorModel(IScreenPage page) : base(page.Icon) {
            Page = page;
        }

    }

}
