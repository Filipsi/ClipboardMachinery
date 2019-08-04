using ClipboardMachinery.Components.ColorGallery;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface IColorGalleryFactory {

        IColorPalette[] GetAllPresets();

        void Release(IColorPalette clipVm);

    }

}
