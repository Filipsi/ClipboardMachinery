using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.TagType;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface IViewModelFactory {

        ClipViewModel CreateClip(ClipModel model);

        TagTypeViewModel CreateTagType(TagTypeModel model);

        void Release(ClipViewModel clip);

        void Release(TagTypeViewModel tagType);

    }

}
