using System.Windows.Media;

namespace ClipboardMachinery.Components.ColorGallery.Presets {

    public class MaterialColorsPreset : IColorPalette {

        public string Name { get; } = "Material";

        public Color[] Colors { get; } = new Color[] {
            Color.FromArgb(255, 244, 67, 54),
            Color.FromArgb(255, 233, 30, 99),
            Color.FromArgb(255, 156, 39, 176),
            Color.FromArgb(255, 103, 58, 183),
            Color.FromArgb(255, 63, 81, 181),
            Color.FromArgb(255, 33, 150, 243),
            Color.FromArgb(255, 3, 169, 244),
            Color.FromArgb(255, 0, 188, 212),
            Color.FromArgb(255, 0, 150, 136),
            Color.FromArgb(255, 76, 175, 80),
            Color.FromArgb(255, 139, 195, 74),
            Color.FromArgb(255, 205, 220, 57),
            Color.FromArgb(255, 255, 235, 59),
            Color.FromArgb(255, 255, 193, 7),
            Color.FromArgb(255, 255, 152, 0),
            Color.FromArgb(255, 255, 87, 34),
            Color.FromArgb(255, 121, 85, 72),
            Color.FromArgb(255, 96, 125, 139)
        };

    }

}
