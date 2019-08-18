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

        private readonly Dictionary<Type, ITagKindSchema> schemaMap;

        #endregion

        public TagKindManager(ITagKindFactory tagKindFactory) {
            ITagKindSchema[] schemas = tagKindFactory.GetAllSchemas();
            schemaMap = schemas.ToDictionary(sch => sch.Kind, sch => sch);

            TagKinds = Array.AsReadOnly(
                schemas
                    .Select(tagKindFactory.CreateTagKind)
                    .Reverse()
                    .ToArray()
            );
        }

        #region Logic

        public ITagKindSchema GetSchemaFor(Type kindType) {
            return schemaMap.ContainsKey(kindType)
                ? schemaMap[kindType]
                : null;
        }

        public bool IsValid(Type kindType, string input) {
            return TryParse(kindType, input, out object result);
        }

        public bool TryParse(Type kindType, string input, out object result) {
            result = null;
            ITagKindSchema tagKindSchema = GetSchemaFor(kindType);
            return tagKindSchema != null && tagKindSchema.TryParse(input, out result);
        }

        #endregion

    }

}
