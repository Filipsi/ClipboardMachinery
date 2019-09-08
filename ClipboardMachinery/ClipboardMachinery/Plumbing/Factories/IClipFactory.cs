using ClipboardMachinery.Components.Tag;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface IClipFactory {

        TagViewModel CreateTag();

        void Release(TagViewModel tag);

    }

}
