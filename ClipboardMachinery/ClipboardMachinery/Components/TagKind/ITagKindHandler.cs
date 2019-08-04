using System;
using System.Collections.Generic;

namespace ClipboardMachinery.Components.TagKind {

    public interface ITagKindHandler {

        IReadOnlyCollection<TagKindViewModel> TagKinds { get; }

        ITagKindSchema FromType(Type kindType);

        bool TryParse(Type kindType, string input, out object result);

    }

}
