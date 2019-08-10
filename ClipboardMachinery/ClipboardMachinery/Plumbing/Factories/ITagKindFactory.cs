using ClipboardMachinery.Components.TagKind;
using ClipboardMachinery.Core.TagKind;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface ITagKindFactory {

        ITagKindSchema[] GetAllSchemas();

        TagKindViewModel CreateTagKind(ITagKindSchema schema);

    }

}
