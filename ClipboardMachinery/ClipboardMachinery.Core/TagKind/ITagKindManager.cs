using System;
using System.Collections.Generic;

namespace ClipboardMachinery.Core.TagKind {

    public interface ITagKindManager {

        IReadOnlyList<ITagKindSchema> Schemas { get; }

        ITagKindSchema GetSchemaFor(Type kindType);

        bool IsValid(Type kindType, string input);

        bool TryParse(Type kindType, string input, out object result);

    }

}
