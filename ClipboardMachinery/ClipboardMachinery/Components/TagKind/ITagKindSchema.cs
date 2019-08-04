using System;

namespace ClipboardMachinery.Components.TagKind {

    public interface ITagKindSchema {

        Type Type { get; }

        string Name { get; }

        string Description { get; }

        string Icon { get; }

        bool TryParse(string value, out object result);

    }

}
