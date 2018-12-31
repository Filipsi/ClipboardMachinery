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
                if (connection == null || connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken) {
                    connection = dbFactory.Open();
                }

                return connection;
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

        private IDbConnection connection;

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

        public async Task InsertClip(string content, DateTime created, KeyValuePair<string, object>[] tags = null) {
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
                            TypeId = tagData.Key,
                            Type = new TagType {
                                Name = tagData.Key,
                                Type = tagData.Value.GetType()
                            },
                            Value = tagData.Value
                        }
                    );
                }
            }
            // Save nested tag references (TagType, etc...)
            foreach (Tag tag in clip.Tags) {
                await connection.SaveAllReferencesAsync(tag);
            }

            // Save clips
            await connection.SaveAsync(clip, references: true);
        }

        public async Task DeleteClip(int id) {
            await connection.DeleteByIdAsync<Clip>(id);
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
