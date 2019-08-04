using System.Windows;
using System.Windows.Controls;

namespace ClipboardMachinery.Common {

    /// <inheritdoc />
    /// <summary>
    /// Adapted from https://wpf.2000things.com/2014/02/19/1012-using-a-different-data-template-for-the-face-of-a-combobox/
    /// </summary>
    public class ComboBoxItemTemplateSelector : DataTemplateSelector {

        #region Properties

        public DataTemplate SelectedItemTemplate { get; set; }

        public DataTemplate ItemTemplate { get; set; }

        #endregion

        #region Logic

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (!(container is FrameworkElement fe)) {
                return ItemTemplate;
            }

            return fe.TemplatedParent is ComboBox ? SelectedItemTemplate : ItemTemplate;
        }

        #endregion

    }

}
