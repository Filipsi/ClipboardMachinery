using ClipboardMachinery.Components.TagKind;
using ClipboardMachinery.Core.TagKind;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface ITagKindFactory {

        TagKindViewModel CreateTagKind(ITagKindSchema schema);

        void Release(TagKindViewModel tagKind);

    }

}
