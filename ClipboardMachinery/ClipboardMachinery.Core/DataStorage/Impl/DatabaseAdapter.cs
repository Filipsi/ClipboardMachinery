using System;
using System.Data;
using Castle.Core.Logging;
using ClipboardMachinery.Core.DataStorage.Schema;
using ServiceStack.OrmLite;

namespace ClipboardMachinery.Core.DataStorage.Impl {

    public class DatabaseAdapter : IDatabaseAdapter {

        #region Properties

        public ILogger Logger { get; set; } = NullLogger.Instance;

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

        public DatabaseAdapter(string databasePath, string databaseVersion, ILogger logger) {
            Logger = logger;
            Logger.Info($"Creating sqlite database adapter with data source bound to '{databasePath}' with version '{databaseVersion}'...");

            // Create connection factory in order to connect to the database
            dbFactory = new OrmLiteConnectionFactory(
                connectionString: $"Data Source={databasePath};Version={databaseVersion};",
                dialectProvider: SqliteDialect.Provider
            );

            // Initialize tables
            EnsureTable<Clip>();
            EnsureTable<Tag>();
            EnsureTable<TagType>();

            // FIXME: Migration from previous db version
            if (Connection.ColumnExists("Created", nameof(Clip))) {
                Logger.Warn("Old clip format detected, removing old Clip table and creating empty one with new data format.");
                Connection.DropAndCreateTable<Clip>();
            }

            // Make sure that  all system owned tag types are in the database
            foreach (TagType systemTagType in SystemTagTypes.TagTypes) {
                if (Connection.Exists<TagType>(new {systemTagType.Name})) {
                    continue;
                }

                Logger.Info($"Mandatory TagType '{systemTagType.Name}' not found, creating entry...");
                Connection.Insert(systemTagType);
            }
        }

        private void EnsureTable<T>() {
            if (Connection.CreateTableIfNotExists<T>()) {
                Logger.Info($"Table of type {typeof(T).Name} not found, creating...");
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
