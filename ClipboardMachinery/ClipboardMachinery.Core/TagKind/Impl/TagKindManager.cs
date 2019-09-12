using System;
using System.Collections.Generic;
using System.Linq;

namespace ClipboardMachinery.Core.TagKind.Impl {

    public class TagKindManager : ITagKindManager {

        #region Properties

        public IReadOnlyList<ITagKindSchema> Schemas { get; }

        #endregion

        #region Fields

        private readonly Dictionary<Type, ITagKindSchema> schemaMap;

        #endregion

        public TagKindManager(ITagKindSchemaFactory kindSchemaFactory) {
            ITagKindSchema[] schemas = kindSchemaFactory.GetAll();
            Schemas = Array.AsReadOnly(schemas);
            schemaMap = schemas.ToDictionary(sch => sch.Kind, sch => sch);
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
