using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Logging;

namespace ClipboardMachinery.Core.TagKind.Impl {

    public class TagKindManager : ITagKindManager {

        #region Properties

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public IReadOnlyList<ITagKindSchema> Schemas { get; }

        #endregion

        #region Fields

        private readonly Dictionary<Type, ITagKindSchema> schemaMap;

        #endregion

        public TagKindManager(ITagKindSchemaFactory kindSchemaFactory, ILogger logger) {
            ITagKindSchema[] schemas = kindSchemaFactory.GetAll();

            Logger = logger;
            Logger.Info("Listing available tag kind schema:");
            foreach (ITagKindSchema tagKindSchema in schemas) {
                Logger.Info($" - Name={tagKindSchema.Name}, Type={tagKindSchema.GetType().FullName}");
            }

            Schemas = Array.AsReadOnly(schemas.Reverse().ToArray());
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
