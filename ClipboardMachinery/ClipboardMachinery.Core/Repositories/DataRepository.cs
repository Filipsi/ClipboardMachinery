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
            connectionString: "Data Source=storage.db;Version=3;",
            dialectProvider: SqliteDialect.Provider
        );

        private IDbConnection connection;

        #endregion

        public DataRepository(IMapper mapper) {
            Mapper = mapper;

            if (Connection.CreateTableIfNotExists<Clip>()) {
                for (int i = 0; i < 128; i++) {
                    Connection.Insert(new Clip {
                        Content = $"clip number: {i}"
                    });
                }
            }
        }

        public ILazyDataProvider CreateLazyClipProvider(int batchSize) {
            return new LazyDataProvider<Clip>(this, batchSize);
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
