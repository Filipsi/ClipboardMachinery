using ClipboardMachinery.Plumbing;
using System.Windows;
using System.Windows.Media;

namespace ClipboardMachinery.Models {

    public class PageButtonModel : ControlModel {

        #region Properties

        public IPage Page {
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
                NotifyOfPropertyChange(nameof(Color));
            }
        }

        #endregion

        #region Fields

        private bool isSelected;

        #endregion

        public PageButtonModel(IPage page) : base(page.Icon) {
            Page = page;
        }

    }

}
