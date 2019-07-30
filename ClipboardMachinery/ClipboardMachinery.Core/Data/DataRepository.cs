using AutoMapper;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ClipboardMachinery.Core.Data.LazyProvider;
using ClipboardMachinery.Core.Data.Schema;

namespace ClipboardMachinery.Core.Data {

    public class DataRepository : IDataRepository {

        #region Properties

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

        public DataRepository(IDatabaseAdapter databaseAdapter, IMapper mapper) {
            Database = databaseAdapter;
            Mapper = mapper;

            // Load last saved clip
            IDbConnection db = Database.Connection;
            SqlExpression <Clip> expression = db.From<Clip>().OrderByDescending(clip => clip.Id);
            LastClipContent = db.Single(expression)?.Content;
        }

        #region IDataRepository

        public ILazyDataProvider CreateLazyClipProvider(int batchSize) {
            return new LazyDataProvider<Clip>(this, batchSize, LoadNestedClipReferences);
        }

        public async Task<T> CreateClip<T>(string content, DateTime created, KeyValuePair<string, object>[] tags = null) {
            // Create clip entity
            Clip clip = new Clip {
                Content = content,
                Created = created,
                Tags = new List<Tag>()
            };

            // Add tags if there are any
            if (tags != null) {
                foreach (KeyValuePair<string, object> tagData in tags) {
                    clip.Tags.Add(
                        new Tag {
                            TypeName = tagData.Key,
                            Value = tagData.Value
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
                            // No need to set color here, default is used if none is set
                        }
                    );
                }

                // Load reference to TagType
                await Database.Connection.LoadReferencesAsync(tag);
            }

            // Save clips
            bool wasSaveSuccessfull = await Database.Connection.SaveAsync(clip, references: true);

            // If we managed to successfully save the clip update last saved clip content
            if (wasSaveSuccessfull) {
                LastClipContent = clip.Content;
            }

            // Map it to the desired model
            return Mapper.Map<T>(clip);
        }

        public async Task DeleteClip(int id) {
            // Delete all related tags
            foreach (Tag relatedTag in await Database.Connection.SelectAsync<Tag>(t => t.ClipId == id)) {
                await Database.Connection.DeleteAsync(relatedTag);
            }

            // Delete the clip itself
            await Database.Connection.DeleteByIdAsync<Clip>(id);
        }

        public async Task<T> CreateTag<T>(int clipId, string type, object value) {
            // Create tag entity
            Tag tag = new Tag {
                ClipId = clipId,
                TypeName = type,
                Value = value
            };

            // Check if TagType exits, if not create it
            if (!await Database.Connection.ExistsAsync<TagType>(new { Name = type })) {
                await Database.Connection.InsertAsync(
                    new TagType {
                        Name = type,
                        Kind = value.GetType(),
                        // No need to set color here, default is used if none is set
                    }
                );
            }

            // Load reference to TagType
            // This is needed to actually fill the newly created tag with TagType values.
            await Database.Connection.LoadReferencesAsync(tag);

            // Save newly created tag
            await Database.Connection.SaveAsync(tag, references: true);

            // Map it to the desired model
            return Mapper.Map<T>(tag);
        }

        public async Task UpdateTag(int id, object value) {
            await Database.Connection.UpdateAsync<Tag>(
                new {
                    Id = id,
                    Value = value
                }
            );
        }

        public async Task DeleteTag(int id) {
            await Database.Connection.DeleteByIdAsync<Tag>(id);
        }

        public ILazyDataProvider CreateLazyTagTypeProvider(int batchSize) {
            return new LazyDataProvider<TagType>(this, batchSize);
        }

        public async Task UpdateTagType(string typeName, System.Windows.Media.Color color) {
            await Database.Connection.UpdateAsync<TagType>(
                new {
                    Name = typeName,
                    Color = Mapper.Map<Color>(color)
                }
            );
        }

        #endregion

        #region Helpers

        private static async Task LoadNestedClipReferences(IDbConnection db, IList<Clip> batch) {
            // Go thought every single clip in the batch
            foreach (Clip clip in batch) {
                // Load nested references for clip tags if there are any
                if (clip.Tags == null) {
                    continue;
                }

                foreach (Tag tag in clip.Tags) {
                    await db.LoadReferencesAsync(tag);
                }
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
