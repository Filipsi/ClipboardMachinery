using AutoMapper;
using ClipboardMachinery.Core.Repositories.Lazy;
using ClipboardMachinery.Core.Repositories.Shema;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.Repositories {

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
        }

        public ILazyDataProvider CreateLazyClipProvider(int batchSize) {
            return new LazyDataProvider<Clip>(this, batchSize);
        }

        public async Task InsertClip(string content, DateTime created, KeyValuePair<string, string>[] tags) {
            Clip clip = new Clip {
                Content = content,
                Created = created,
                Tags = new List<Tag>()
            };

            if (tags != null) {
                foreach (KeyValuePair<string, string> tagData in tags) {
                    Tag tag = new Tag {
                        Value = tagData.Value,
                        Type = new TagType {
                            Name = tagData.Key,
                            Type = tagData.Key.GetType()
                        },
                    };
                }
            }

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
