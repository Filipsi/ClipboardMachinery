using System;
using System.Collections.Generic;
using System.Linq;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Components.TagKind {

    public class TagKindHandler : ITagKindHandler {

        #region Properties

        internal IReadOnlyCollection<ITagKindSchema> Schemas { get; }

        public IReadOnlyCollection<TagKindViewModel> TagKinds { get; }

        #endregion

        public TagKindHandler(ITagKindFactory tagKindFactory) {
            Schemas = Array.AsReadOnly(
                tagKindFactory.GetAllSchemas()
            );

            TagKinds = Array.AsReadOnly(
                Schemas
                    .Select(tagKindFactory.CreateTagKind)
                    .Reverse()
                    .ToArray()
            );
        }

        #region Logic

        public ITagKindSchema FromType(Type kindType) {
            return Schemas.FirstOrDefault(schema => schema.Type == kindType);
        }

        public bool TryParse(Type kindType, string input, out object result) {
            result = null;
            ITagKindSchema tagKindSchema = FromType(kindType);
            return tagKindSchema != null && tagKindSchema.TryParse(input, out result);
        }

        #endregion

    }

}
