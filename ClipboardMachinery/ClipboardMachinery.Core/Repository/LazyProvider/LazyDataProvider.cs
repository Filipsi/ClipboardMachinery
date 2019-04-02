using System.Collections.Generic;
using ServiceStack.OrmLite;
using System.Threading.Tasks;
using System;
using ServiceStack.OrmLite.Legacy;
using System.Data;

namespace ClipboardMachinery.Core.Repository.LazyProvider {

    public class LazyDataProvider<T> : ILazyDataProvider {

        #region Fields

        private readonly DataRepository dataRepository;
        private readonly int batchSize;
        private readonly Func<IDbConnection, IList<T>, Task> onBatchLoaded;

        private int offset = 0;

        #endregion

        internal LazyDataProvider(DataRepository dataRepository, int batchSize, Func<IDbConnection, IList<T>, Task> onBatchLoaded = null) {
            this.dataRepository = dataRepository;
            this.batchSize = batchSize;
            this.onBatchLoaded = onBatchLoaded;
        }

        #region Logic

        public async Task<IEnumerable<M>> GetNextBatchAsync<M>() {
            IDbConnection db = dataRepository.Connection;

            // Create SQL query
            SqlExpression<T> query = db.From<T>().OrderByDescending("id").Limit(batchSize);
            query.Offset = offset;

            // Load entries of type T
            List<T> entries = await db.LoadSelectAsync(query);

            // Run batch creation hooks
            // This is used to perform type specific action when batch is loaded from the database
            // For example to load nested references
            if (onBatchLoaded != null) {
                await onBatchLoaded.Invoke(db, entries);
            }

            // Move offset of lazy loader
            offset += entries.Count;

            // Map T results to desired models
            return dataRepository.Mapper.Map<M[]>(entries);
        }

        public void Reset() {
            offset = 0;
        }

        #endregion

    }

}
