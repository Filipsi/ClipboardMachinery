using System;

namespace ClipboardMachinery.Core.TagKind {

    public interface ITagKindSchema {

        Type Kind { get; }

        string Name { get; }

        string Description { get; }

        string Icon { get; }

        bool TryParse(string value, out object result);

        string ToDisplayValue(object value);

    }

}
