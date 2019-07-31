

using System.Windows.Media;

namespace ClipboardMachinery.Components.ColorGallery.Presets {

    public class FlatColorsPreset : IColorPalette {

        public string Name { get; } = "Flat";

        public Color[] Colors { get; } = {
            Color.FromArgb(255, 26, 188, 156),
            Color.FromArgb(255, 22, 160, 133),
            Color.FromArgb(255, 46, 204, 113),
            Color.FromArgb(255, 39, 174, 96),
            Color.FromArgb(255, 52, 152, 219),
            Color.FromArgb(255, 41, 128, 185),
            Color.FromArgb(255, 155, 89, 182),
            Color.FromArgb(255, 142, 68, 173),
            Color.FromArgb(255, 241, 196, 15),
            Color.FromArgb(255, 243, 156, 18),
            Color.FromArgb(255, 230, 126, 34),
            Color.FromArgb(255, 211, 84, 0),
            Color.FromArgb(255, 231, 76, 60),
            Color.FromArgb(255, 192, 57, 43)
        };

    }
}
