using ClipboardMachinery.Components.ColorGallery.Presets;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface IColorGalleryFactory {

        IColorPalette[] GetAllPresets();

        void Release(IColorPalette clipVm);

    }

}
