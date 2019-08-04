using ClipboardMachinery.Components.TagKind;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface ITagKindFactory {

        ITagKindSchema[] GetAllSchemas();

        TagKindViewModel CreateTagKind(ITagKindSchema schema);

    }

}
