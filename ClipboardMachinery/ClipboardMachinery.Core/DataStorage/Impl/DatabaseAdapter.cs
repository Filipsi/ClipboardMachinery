using System;
using System.Data;
using ClipboardMachinery.Core.DataStorage.Schema;
using ServiceStack.OrmLite;

namespace ClipboardMachinery.Core.DataStorage.Impl {

    public class DatabaseAdapter : IDatabaseAdapter {

        #region Properties

        public IDbConnection Connection {
            get {
                if (!IsConnectionOpen) {
                    db = dbFactory.Open();
                }

                return db;
            }
        }

        public bool IsConnectionOpen
            => db != null && (db.State != ConnectionState.Closed || db.State != ConnectionState.Broken);

        public int Version
            => Connection.Scalar<int>("PRAGMA user_version");

        #endregion

        #region Fields

        private readonly OrmLiteConnectionFactory dbFactory;
        private IDbConnection db;

        #endregion

        public DatabaseAdapter(string dataSourcePath) {
            // Create connection factory in order to connect to the database
            dbFactory = new OrmLiteConnectionFactory(
                connectionString: $"Data Source={dataSourcePath};Version=3;",
                dialectProvider: SqliteDialect.Provider
            );

            // Initialize tables
            Connection.CreateTableIfNotExists<Clip>();
            Connection.CreateTableIfNotExists<Tag>();
            Connection.CreateTableIfNotExists<TagType>();

            // Make sure that there are all system owned tag types
            foreach (TagType systemTagType in SystemTagTypes.TagTypes) {
                if (!Connection.Exists<TagType>(new { systemTagType.Name })) {
                    Connection.Insert(systemTagType);
                }
            }
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
