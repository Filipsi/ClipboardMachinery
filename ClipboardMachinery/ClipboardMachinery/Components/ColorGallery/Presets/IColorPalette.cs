using System.Windows.Media;

namespace ClipboardMachinery.Components.ColorGallery.Presets {

    public interface IColorPalette {

        string Name { get; }

        Color[] Colors { get; }

    }

}
