using ClipboardMachinery.Components.Clip;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface IClipViewModelFactory {

        ClipViewModel Create(ClipModel model);

        void Release(ClipViewModel clipVm);

    }

}
