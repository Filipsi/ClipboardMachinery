using System;
using System.Collections.Generic;
using ClipboardMachinery.Core.TagKind;

namespace ClipboardMachinery.Components.TagKind {

    public interface ITagKindManager {

        IReadOnlyCollection<TagKindViewModel> TagKinds { get; }

        ITagKindSchema GetSchemaFor(Type kindType);

        bool IsValid(Type kindType, string input);

        bool TryParse(Type kindType, string input, out object result);

    }

}
