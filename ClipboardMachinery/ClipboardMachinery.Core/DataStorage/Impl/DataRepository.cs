using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Castle.Core.Logging;
using ClipboardMachinery.Core.DataStorage.Schema;
using ClipboardMachinery.Core.TagKind;
using ServiceStack.OrmLite;
using Color = ClipboardMachinery.Core.DataStorage.Schema.Color;
using MediaColor = System.Windows.Media.Color;

namespace ClipboardMachinery.Core.DataStorage.Impl {

    public class DataRepository : IDataRepository {

        #region Properties

        public ILogger Logger { get; set; } = NullLogger.Instance;

        internal IDatabaseAdapter Database {
            get;
        }

        internal IMapper Mapper {
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

        public DataRepository(IDatabaseAdapter databaseAdapter, IMapper mapper, ITagKindManager tagKindManager) {
            Database = databaseAdapter;
            Mapper = mapper;
            this.tagKindManager = tagKindManager;

            // Create data providers list to tract instances
            dataProviders = new List<WeakReference<ILazyDataProvider>>();

            // Load last saved clip
            IDbConnection db = Database.Connection;
            SqlExpression <Clip> expression = db.From<Clip>().OrderByDescending(clip => clip.Id);
            LastClipContent = db.Single(expression)?.Content;
        }

        #region IDataRepository

        public ClipLazyProvider CreateLazyClipProvider(int batchSize) {
            ClipLazyProvider clipProvider = new ClipLazyProvider(this, batchSize);
            dataProviders.Add(new WeakReference<ILazyDataProvider>(clipProvider));
            return clipProvider;
        }

        public async Task<T> CreateClip<T>(string content, string contentPresenter, KeyValuePair<string, object>[] tags = null) {
            // Create clip entity
            Clip clip = new Clip {
                Content = content,
                Presenter = contentPresenter,
                Tags = new List<Tag>()
            };

            // Add tags if there are any
            if (tags != null) {
                foreach (KeyValuePair<string, object> tagData in tags) {
                    string presistentValue = await ResolvePresistentValue(tagData.Key, tagData.Value);

                    if (presistentValue == null) {
                        Logger.Error($"Unable to create tag {tagData.Key}={tagData.Value} for clip: {content}");
                        continue;
                    }

                    clip.Tags.Add(
                        new Tag {
                            TypeName = tagData.Key,
                            Value = presistentValue
                        }
                    );
                }
            }

            // Handle tag type for every new tag
            foreach (Tag tag in clip.Tags) {
                // Check if TagType the Tag is specifying exits, if not create it
                if (!await Database.Connection.ExistsAsync<TagType>(new { Name = tag.TypeName })) {
                    await Database.Connection.InsertAsync(
                        new TagType {
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
            bool wasSaveSuccessfull = await Database.Connection.SaveAsync(clip, references: true);

            // If we managed to successfully save the clip update last saved clip content
            // ReSharper disable once InvertIf
            if (wasSaveSuccessfull) {
                LastClipContent = clip.Content;
                await UpdateDataProvidersOffset<Clip>(1);
            } else {
                Logger.Error($"Unable to save clip: {content}");
            }

            // Map it to the desired model
            return Mapper.Map<T>(clip);
        }

        public Task UpdateClip(int id, string contentPresenter) {
            return Database.Connection.UpdateAsync<Clip>(
                new {
                    Id = id,
                    Presenter = contentPresenter
                }
            );
        }

        public async Task DeleteClip(int id) {
            // Delete all related tags
            foreach (Tag relatedTag in await Database.Connection.SelectAsync<Tag>(t => t.ClipId == id)) {
                await Database.Connection.DeleteAsync(relatedTag);
            }

            // Delete the clip itself
            await Database.Connection.DeleteByIdAsync<Clip>(id);
        }

        public async Task<T> CreateTag<T>(int clipId, string tagType, object value) {
            string presistentValue = await ResolvePresistentValue(tagType, value);

            if (presistentValue == null) {
                Logger.Error($"Unable to create tag {tagType}={value} for clip id '{clipId}'!");
                return default;
            }

            // Create tag entity
            Tag tag = new Tag {
                ClipId = clipId,
                TypeName = tagType,
                Value = presistentValue
            };

            // Check if TagType exits, if not create it
            if (!await Database.Connection.ExistsAsync<TagType>(new { Name = tagType })) {
                await Database.Connection.InsertAsync(
                    new TagType {
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
            await UpdateDataProvidersOffset<Tag>(1);

            // Map it to the desired model
            return Mapper.Map<T>(tag);
        }

        public async Task<T> FindTag<T>(int tagId) {
            List<Tag> foundTags = await Database.Connection.SelectAsync<Tag>(
                tag => tag.Id == tagId
            );

            Tag firstMatch = foundTags.FirstOrDefault();

            if (firstMatch == null) {
                Logger.Error($"Unable to find tag with id '{tagId}'!");
                return default;
            }

            await Database.Connection.LoadReferencesAsync(firstMatch);
            return Mapper.Map<T>(firstMatch);
        }

        public async Task<string> UpdateTag(int id, object value) {
            Tag tag = await FindTag<Tag>(id);

            if (tag == null) {
                Logger.Error($"Unable to update tag with id '{id}'!");
                return string.Empty;
            }

            string presistentValue = await ResolvePresistentValue(tag.Type.Name, value);

            if (presistentValue == null) {
                Logger.Error($"Unable to save updated tag with id '{id}' due to failure while parsing value '{value}' for tag type '{tag.Type.Name}'!");
                return string.Empty;
            }

            await Database.Connection.UpdateAsync<Tag>(
                new {
                    Id = id,
                    Value = presistentValue
                }
            );

            return presistentValue;
        }

        public async Task DeleteTag(int id) {
            await Database.Connection.DeleteByIdAsync<Tag>(id);
        }

        public ILazyDataProvider CreateLazyTagTypeProvider(int batchSize) {
            ILazyDataProvider tagTypeProvider = new GenericLazyProvider<TagType>(this, batchSize);
            dataProviders.Add(new WeakReference<ILazyDataProvider>(tagTypeProvider));
            return tagTypeProvider;
        }

        public async Task<T> CreateTagType<T>(string name, string description, Type kind, MediaColor? color = null) {
            // Check if there is already tag type with this name
            if (await TagTypeExists(name)) {
                Logger.Error($"Unable to create tag type with '{name}', tag type with this name already exists!");
                return default;
            }

            // Create new tag type
            TagType tagType = new TagType {
                Name = name,
                Description = description,
                Kind = kind,
                Color = color.HasValue
                    ? new Color {A = color.Value.A, R = color.Value.R, G = color.Value.G, B = color.Value.B}
                    : SystemTagTypes.DefaultDBColor
            };

            // Save newly created tag type
            await Database.Connection.InsertAsync(tagType);
            await UpdateDataProvidersOffset<TagType>(1);

            // Map it to the desired model
            return Mapper.Map<T>(tagType);
        }

        public async Task<bool> TagTypeExists(string name) {
            return await Database.Connection.ExistsAsync<TagType>(
                tagType => tagType.Name == name
            );
        }

        public async Task<T> FindTagType<T>(string name) {
            List<TagType> foundTypes = await Database.Connection.SelectAsync<TagType>(
                tagType => tagType.Name == name
            );

            TagType firstMatch = foundTypes.FirstOrDefault();

            // ReSharper disable once InvertIf
            if (firstMatch == null) {
                Logger.Error($"Unable to find tag type with name '{name}'!");
                return default;
            }

            return Mapper.Map<T>(firstMatch);
        }

        public async Task UpdateTagType(string name, MediaColor color) {
            await Database.Connection.UpdateAsync<TagType>(
                new {
                    Name = name,
                    Color = Mapper.Map<Color>(color)
                }
            );
        }

        public async Task UpdateTagType(string name, string description) {
            await Database.Connection.UpdateAsync<TagType>(
                new {
                    Name = name,
                    Description = description
                }
            );
        }

        public async Task DeleteTagType(string name) {
            if (!await TagTypeExists(name)) {
                Logger.Error($"Unable to delete tag type with name '{name}', there is not tag type matching this name!");
                return;
            }

            await Database.Connection.DeleteAsync<Tag>(
                tag => tag.TypeName == name
            );

            await Database.Connection.DeleteAsync<TagType>(
                tagType => tagType.Name == name
            );
        }

        #endregion

        #region Helpers

        private async Task<string> ResolvePresistentValue(string tagType, object value) {
            TagType ttype = await FindTagType<TagType>(tagType);

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

            DispatchToDataProvders<T>(dataProvider => {
                // Skip this provider if it does not have all entries loaded.
                // The offset should be updated only when new entry is added or existing one is removed.
                if (dataProvider.Offset != entryCount - value) {
                    return;
                }

                // Update the offset of the provider
                dataProvider.Offset += value;
            });
        }

        private void DispatchToDataProvders<T>(Action<ILazyDataProvider> action) {
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
