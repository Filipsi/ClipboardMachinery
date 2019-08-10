using System;
using System.Collections.Generic;
using System.Linq;
using ClipboardMachinery.Core.TagKind;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Components.TagKind {

    public class TagKindManager : ITagKindManager {

        #region Properties

        public IReadOnlyCollection<TagKindViewModel> TagKinds { get; }

        #endregion

        #region Fields

        private readonly IReadOnlyCollection<ITagKindSchema> schemas;

        #endregion

        public TagKindManager(ITagKindFactory tagKindFactory) {
            schemas = Array.AsReadOnly(
                tagKindFactory.GetAllSchemas()
            );

            TagKinds = Array.AsReadOnly(
                schemas
                    .Select(tagKindFactory.CreateTagKind)
                    .Reverse()
                    .ToArray()
            );
        }

        #region Logic

        public ITagKindSchema GetSchemaFor(Type kindType) {
            return schemas.FirstOrDefault(schema => schema.Type == kindType);
        }

        public bool TryParse(Type kindType, string input, out object result) {
            result = null;
            ITagKindSchema tagKindSchema = GetSchemaFor(kindType);
            return tagKindSchema != null && tagKindSchema.TryParse(input, out result);
        }

        #endregion

    }

}
