using System;

namespace ClipboardMachinery.Core.TagKind {

    public interface ITagKindSchema {

        Type Type { get; }

        string Name { get; }

        string Description { get; }

        string Icon { get; }

        bool TryParse(string value, out object result);

    }

}
