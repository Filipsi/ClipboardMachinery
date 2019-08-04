using System.Windows.Media;

namespace ClipboardMachinery.Components.ColorGallery {

    public interface IColorPalette {

        string Name { get; }

        Color[] Colors { get; }

    }

}
