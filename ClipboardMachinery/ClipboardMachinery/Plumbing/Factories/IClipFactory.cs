using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Components.TagType;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface IClipFactory {

        ClipViewModel CreateClip(ClipModel model);

        TagViewModel CreateTag(TagModel tagModel);

        TagTypeViewModel CreateTagType(TagTypeModel model);

        void Release(ClipViewModel clip);

        void Release(TagViewModel tag);

        void Release(TagTypeViewModel tagType);

    }

}
