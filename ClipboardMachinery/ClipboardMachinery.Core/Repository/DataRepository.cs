using AutoMapper;
using ClipboardMachinery.Core.Repository.LazyProvider;
using ClipboardMachinery.Core.Repository.Schema;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ClipboardMachinery.Core.Repository {

    public class DataRepository : IDataRepository {

        #region Properties

        internal IDbConnection Connection {
            get {
                if (db == null || db.State == ConnectionState.Closed || db.State == ConnectionState.Broken) {
                    db = dbFactory.Open();
                }

                return db;
            }
        }

        internal IMapper Mapper {
            get;
        }

        #endregion

        #region Fields

        private readonly OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory(
            connectionString: "Data Source=storage.sqlite;Version=3;",
            dialectProvider: SqliteDialect.Provider
        );

        private IDbConnection db;

        #endregion

        public DataRepository(IMapper mapper) {
            Mapper = mapper;

            // Initialize tables
            Connection.CreateTableIfNotExists<Clip>();
            Connection.CreateTableIfNotExists<Tag>();
            Connection.CreateTableIfNotExists<TagType>();
        }

        public ILazyDataProvider CreateLazyClipProvider(int batchSize) {
            return new LazyDataProvider<Clip>(this, batchSize);
        }

        public async Task<T> InsertClip<T>(string content, DateTime created, KeyValuePair<string, object>[] tags = null) {
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
                if (!await db.ExistsAsync<TagType>(new { Name = tag.TypeName })) {
                    await db.InsertAsync(
                        new TagType {
                            Name = tag.TypeName,
                            Type = tag.Value.GetType(),
                            // No need to set color here, default is used if none is set
                        }
                    );
                }

                // Load reference to TagType
                await db.LoadReferencesAsync(tag);
            }

            // Save clips
            await db.SaveAsync(clip, references: true);
            return Mapper.Map<T>(clip);
        }

        public async Task DeleteClip(int id) {
            // Delete all related tags
            foreach(Tag relatedTag in await db.SelectAsync<Tag>(t => t.ClipId == id)) {
                await db.DeleteAsync(relatedTag);
            }

            // Delete the clip itself
            await db.DeleteByIdAsync<Clip>(id);
        }

        public async Task DeleteTag(int id) {
            await db.DeleteByIdAsync<Tag>(id);
        }

        public async Task UpdateTag(int id, object value) {
            await db.UpdateAsync<Tag>(
                new {
                    Id = id,
                    Value = value
                }
            );
        }

        public async Task UpdateTagProperty(string name, System.Windows.Media.Color color) {
            await db.UpdateAsync<TagType>(
                new {
                    Id = name,
                    Color = Mapper.Map<Schema.Color>(color)
                }
            );
        }

        #region IDisposable

        private bool isDisposed;

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!isDisposed) {
                if (disposing) {
                    Connection.Dispose();
                }

                // There are no unmanaged resources to release, but
                // if we add them, they need to be released here.
            }

            isDisposed = true;
        }

        #endregion

    }

}
