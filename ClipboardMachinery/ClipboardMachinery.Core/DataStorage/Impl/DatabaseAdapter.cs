using System;
using System.Data;
using Castle.Core.Logging;
using ClipboardMachinery.Core.DataStorage.Schema;
using Microsoft.Extensions.DependencyInjection;
using FluentMigrator.Runner;
using ServiceStack.OrmLite;
using NLog.Extensions.Logging;

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

        public bool IsConnectionOpen {
            get => db != null && (db.State != ConnectionState.Closed || db.State != ConnectionState.Broken);
        }

        public int Version {
            get => Connection.Scalar<int>("PRAGMA user_version");
        }

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

            // Upgrade datebase if necessary
            using (IServiceScope scope = SetupFluentMigratorServices().CreateScope()) {
                UpdateDatabase(scope.ServiceProvider);
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

        #region Logic

        private IServiceProvider SetupFluentMigratorServices() {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(dbFactory.ConnectionString)
                    .ScanIn(typeof(DatabaseAdapter).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddNLog())
                .BuildServiceProvider(false);
        }

        private void EnsureTable<T>() {
            if (Connection.CreateTableIfNotExists<T>()) {
                Logger.Info($"Table of type {typeof(T).Name} not found, creating...");
            }
        }

        private void UpdateDatabase(IServiceProvider serviceProvider) {
            try {
                serviceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();
            } catch(Exception ex) {
                Logger.Error("Failed to upgrade database!", ex);
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
