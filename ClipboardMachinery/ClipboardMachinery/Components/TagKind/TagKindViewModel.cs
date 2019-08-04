using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;

namespace ClipboardMachinery.Components.TagKind {

    public class TagKindViewModel : Screen {

        #region Properties

        public ITagKindSchema Schema {
            get;
        }

        public Geometry Icon
            => (Geometry)Application.Current.TryFindResource(Schema.Icon);

        #endregion

        public TagKindViewModel(ITagKindSchema schema) {
            Schema = schema;
        }

    }

}
