using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ClipboardMachinery.Core.DataStorage.Schema;
using ClipboardMachinery.Core.TagKind;
using ServiceStack.OrmLite;
using System.Windows.Media;

namespace ClipboardMachinery.Core.DataStorage.Impl {

    public class DataRepository : IDataRepository {

        #region Properties

        public ILogger Logger { get; set; } = NullLogger.Instance;

        internal IDatabaseAdapter Database {
            get;
        }

        public string LastClipContent {
            get;
            private set;
        }

        #endregion

        #region Fields

        private readonly ITagKindManager tagKindManager;
        private readonly List<WeakReference<ILazyDataProvider>> dataProviders;

        #endregion

        public DataRepository(IDatabaseAdapter databaseAdapter, ITagKindManager tagKindManager) {
            Database = databaseAdapter;
            this.tagKindManager = tagKindManager;

            // Create data providers list to tract instances
            dataProviders = new List<WeakReference<ILazyDataProvider>>();

            // Load last saved clip
            IDbConnection db = Database.Connection;
            SqlExpression <ClipEntity> expression = db.From<ClipEntity>().OrderByDescending(clip => clip.Id);
            LastClipContent = db.Single(expression)?.Content;
        }

        #region IDataRepository

        public ClipLazyProvider CreateLazyClipProvider(int batchSize) {
            ClipLazyProvider clipProvider = new ClipLazyProvider(this, batchSize);
            dataProviders.Add(new WeakReference<ILazyDataProvider>(clipProvider));
            return clipProvider;
        }

        public async Task<ClipEntity> CreateClip(string content, string contentPresenter, KeyValuePair<string, object>[] tags = null) {
            // Create clip entity
            ClipEntity clip = new ClipEntity {
                Content = content,
                Presenter = contentPresenter,
                Tags = new List<TagEntity>()
            };

            // Add tags if there are any
            if (tags != null) {
                foreach (KeyValuePair<string, object> tagData in tags) {
                    string persistentValue = await ResolvePersistentValue(tagData.Key, tagData.Value);

                    if (persistentValue == null) {
                        Logger.Error($"Unable to create tag {tagData.Key}={tagData.Value} for clip: {content}");
                        continue;
                    }

                    clip.Tags.Add(
                        new TagEntity {
                            TypeName = tagData.Key,
                            Value = persistentValue
                        }
                    );
                }
            }

            // Handle tag type for every new tag
            foreach (TagEntity tag in clip.Tags) {
                // Check if TagType the Tag is specifying exits, if not create it
                if (!await Database.Connection.ExistsAsync<TagTypeEntity>(new { Name = tag.TypeName })) {
                    await Database.Connection.InsertAsync(
                        new TagTypeEntity {
                            Name = tag.TypeName,
                            Kind = tag.Value.GetType(),
                            Color = SystemTagTypes.DefaultDBColor
                        }
                    );
                }

                // Load reference to TagType
                await Database.Connection.LoadReferencesAsync(tag);
            }

            // Save clips
            bool wasSaveSuccessful = await Database.Connection.SaveAsync(clip, references: true);

            // If we managed to successfully save the clip update last saved clip content
            // ReSharper disable once InvertIf
            if (wasSaveSuccessful) {
                LastClipContent = clip.Content;
                await UpdateDataProvidersOffset<ClipEntity>(1);
            } else {
                Logger.Error($"Unable to save clip: {content}");
            }

            return clip;
        }

        public Task UpdateClip(int id, string contentPresenter) {
            return Database.Connection.UpdateAsync<ClipEntity>(
                new {
                    Id = id,
                    Presenter = contentPresenter
                }
            );
        }

        public async Task DeleteClip(int id) {
            // Delete all related tags
            foreach (TagEntity relatedTag in await Database.Connection.SelectAsync<TagEntity>(t => t.ClipId == id)) {
                await Database.Connection.DeleteAsync(relatedTag);
            }

            // Delete the clip itself
            await Database.Connection.DeleteByIdAsync<ClipEntity>(id);
        }

        public async Task<TagEntity> CreateTag(int clipId, string tagType, object value) {
            string persistentValue = await ResolvePersistentValue(tagType, value);

            if (persistentValue == null) {
                Logger.Error($"Unable to create tag {tagType}={value} for clip id '{clipId}'!");
                return default;
            }

            // Create tag entity
            TagEntity tag = new TagEntity {
                ClipId = clipId,
                TypeName = tagType,
                Value = persistentValue
            };

            // Check if TagType exits, if not create it
            if (!await Database.Connection.ExistsAsync<TagTypeEntity>(new { Name = tagType })) {
                await Database.Connection.InsertAsync(
                    new TagTypeEntity {
                        Name = tagType,
                        Kind = value.GetType(),
                        Color = SystemTagTypes.DefaultDBColor
                    }
                );
            }

            // Load reference to TagType
            // This is needed to actually fill the newly created tag with TagType values.
            await Database.Connection.LoadReferencesAsync(tag);

            // Save newly created tag
            await Database.Connection.SaveAsync(tag, references: true);
            await UpdateDataProvidersOffset<TagEntity>(1);

            return tag;
        }

        public async Task<TagEntity> FindTag(int tagId) {
            List<TagEntity> foundTags = await Database.Connection.SelectAsync<TagEntity>(
                tag => tag.Id == tagId
            );

            TagEntity firstMatch = foundTags.FirstOrDefault();

            if (firstMatch == null) {
                Logger.Error($"Unable to find tag with id '{tagId}'!");
                return default;
            }

            await Database.Connection.LoadReferencesAsync(firstMatch);
            return firstMatch;
        }

        public async Task<string> UpdateTag(int id, object value) {
            TagEntity tag = await FindTag(id);

            if (tag == null) {
                Logger.Error($"Unable to update tag with id '{id}'!");
                return string.Empty;
            }

            string persistentValue = await ResolvePersistentValue(tag.Type.Name, value);

            if (persistentValue == null) {
                Logger.Error($"Unable to save updated tag with id '{id}' due to failure while parsing value '{value}' for tag type '{tag.Type.Name}'!");
                return string.Empty;
            }

            await Database.Connection.UpdateAsync<TagEntity>(
                new {
                    Id = id,
                    Value = persistentValue
                }
            );

            return persistentValue;
        }

        public async Task DeleteTag(int id) {
            await Database.Connection.DeleteByIdAsync<TagEntity>(id);
        }

        public ILazyDataProvider<TagTypeEntity> CreateLazyTagTypeProvider(int batchSize) {
            ILazyDataProvider<TagTypeEntity> tagTypeProvider = new GenericLazyProvider<TagTypeEntity>(this, batchSize);
            dataProviders.Add(new WeakReference<ILazyDataProvider>(tagTypeProvider));
            return tagTypeProvider;
        }

        public async Task<TagTypeEntity> CreateTagType(string name, string description, Type kind, byte priority = 0, Color? color = null) {
            // Check if there is already tag type with this name
            if (await TagTypeExists(name)) {
                Logger.Error($"Unable to create tag type with '{name}', tag type with this name already exists!");
                return default;
            }

            // Create new tag type
            TagTypeEntity tagType = new TagTypeEntity {
                Name = name,
                Description = description,
                Kind = kind,
                Priority = priority,
                Color = color.HasValue
                    ? new ColorEntity {A = color.Value.A, R = color.Value.R, G = color.Value.G, B = color.Value.B}
                    : SystemTagTypes.DefaultDBColor
            };

            // Save newly created tag type
            await Database.Connection.InsertAsync(tagType);
            await UpdateDataProvidersOffset<TagTypeEntity>(1);

            return tagType;
        }

        public async Task<bool> TagTypeExists(string name) {
            return await Database.Connection.ExistsAsync<TagTypeEntity>(
                tagType => tagType.Name == name
            );
        }

        public async Task<TagTypeEntity> FindTagType(string name) {
            List<TagTypeEntity> foundTypes = await Database.Connection.SelectAsync<TagTypeEntity>(
                tagType => tagType.Name == name
            );

            TagTypeEntity firstMatch = foundTypes.FirstOrDefault();

            // ReSharper disable once InvertIf
            if (firstMatch == null) {
                Logger.Error($"Unable to find tag type with name '{name}'!");
                return default;
            }

            return firstMatch;
        }

        public async Task UpdateTagType(string name, string description, byte? priority, Color? color) {
            Dictionary<string, object> fields = new Dictionary<string, object> {
                { "Name", name }
            };

            if (description != null) {
                fields.Add(nameof(TagTypeEntity.Description), description);
            }

            if (priority.HasValue) {
                fields.Add(nameof(TagTypeEntity.Priority), priority.Value);
            }

            if (color.HasValue) {
                fields.Add(nameof(TagTypeEntity.Color), new ColorEntity { A = color.Value.A, R = color.Value.R, G = color.Value.G, B = color.Value.B });
            }

            await Database.Connection.UpdateOnlyAsync<TagTypeEntity>(fields);
        }

        public async Task DeleteTagType(string name) {
            if (!await TagTypeExists(name)) {
                Logger.Error($"Unable to delete tag type with name '{name}', there is not tag type matching this name!");
                return;
            }

            await Database.Connection.DeleteAsync<TagEntity>(
                tag => tag.TypeName == name
            );

            await Database.Connection.DeleteAsync<TagTypeEntity>(
                tagType => tagType.Name == name
            );
        }

        #endregion

        #region Helpers

        private async Task<string> ResolvePersistentValue(string tagType, object value) {
            TagTypeEntity ttype = await FindTagType(tagType);

            // Skip tag, non-existent tag type
            if (ttype == null) {
                Logger.Error($"Unable resolve persistent value for tag type with name '{tagType}', no tag type with this name was found!");
                return null;
            }

            ITagKindSchema schema = tagKindManager.GetSchemaFor(ttype.Kind);

            // Skip tag if there is no schema that would allow parsing the value
            // ReSharper disable once ConvertIfStatementToReturnStatement
            // ReSharper disable once UseNullPropagation
            // ReSharper disable once InvertIf
            if (schema == null) {
                Logger.Error($"Unable resolve persistent value for tag type with name '{tagType}', no schema defined for data kind '{ttype.Kind.Name}'!");
                return null;
            }

            return schema.ToPersistentValue(value);
        }

        private async Task UpdateDataProvidersOffset<T>(int value) {
            // TODO: Change this to "SELECT MAX(_ROWID_) FROM "table" LIMIT 1;" depending on performance
            long entryCount = await Database.Connection.CountAsync<T>();

            DispatchToDataProviders<T>(dataProvider => {
                // Skip this provider if it does not have all entries loaded.
                // The offset should be updated only when new entry is added or existing one is removed.
                if (dataProvider.Offset != entryCount - value) {
                    return;
                }

                // Update the offset of the provider
                dataProvider.Offset += value;
            });
        }

        private void DispatchToDataProviders<T>(Action<ILazyDataProvider> action) {
            foreach (WeakReference<ILazyDataProvider> providerRef in dataProviders.ToArray()) {
                // Try to get reference target, if there is no target remove it from tracked providers pool
                if (!providerRef.TryGetTarget(out ILazyDataProvider lazyDataProvider)) {
                    dataProviders.Remove(providerRef);
                    continue;
                }

                // Check if this provider is compatible with target type
                if (lazyDataProvider.DataType != typeof(T)) {
                    continue;
                }

                // Perform the action on target provider
                action?.Invoke(lazyDataProvider);
            }
        }

        #endregion

        #region IDisposable

        private bool isDisposed;

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!isDisposed) {
                if (disposing) {
                    Database.Dispose();
                }

                // There are no unmanaged resources to release, but
                // if we add them, they need to be released here.
            }

            isDisposed = true;
        }

        #endregion

    }

}
