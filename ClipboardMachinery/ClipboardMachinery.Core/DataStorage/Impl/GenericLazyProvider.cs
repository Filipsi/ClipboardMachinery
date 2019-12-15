using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceStack.OrmLite;

namespace ClipboardMachinery.Core.DataStorage.Impl {

    public class GenericLazyProvider<T> : ILazyDataProvider {

        #region Properties

        public Type DataType { get; } = typeof(T);

        public int BatchSize { get; }

        public int Offset { get; set; }

        #endregion

        #region Fields

        private readonly DataRepository dataRepository;

        #endregion

        internal GenericLazyProvider(DataRepository dataRepository, int batchSize) {
            this.dataRepository = dataRepository;
            BatchSize = batchSize;
        }

        #region Logic

        public async Task<IEnumerable<TM>> GetNextBatchAsync<TM>() {
            // Keep reference to database connection
            IDbConnection db = dataRepository.Database.Connection;

            // Create SQL query
            SqlExpression<T> query = db.From<T>();

            // Apply additional, type specific actions on query creation
            await OnQueryBuildingStarts(query);

            // Apply limit and offset to only load slice of the state
            query.Limit(BatchSize);
            query.Offset = Offset;

            // Load entries of type T
            List<T> entries = await db.LoadSelectAsync(query);

            // Apply additional, type specific actions on the batch of results
            await OnBatchLoaded(db, entries);

            // Move offset of lazy loader
            Offset += entries.Count;

            // Map T results to desired models
            return dataRepository.Mapper.Map<TM[]>(entries);
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// Lifecycle method called when new SQL expression stats been built.
        /// </summary>
        /// <param name="query">An SQL expression been built.</param>
        /// <returns>A task completed upon lifecycle method finishes executing.</returns>
        protected virtual Task OnQueryBuildingStarts(SqlExpression<T> query) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Lifecycle method used when ordering SQL expression currently been built.
        /// </summary>
        /// <param name="query">An SQL expression been built.</param>
        /// <returns>A task completed upon lifecycle method finishes executing.</returns>
        protected virtual Task OnQueryOrdering(SqlExpression<T> query) {
            query.OrderByDescending(1);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Lifecycle method that can be used to perform type specific action when batch is loaded from the database.
        /// For example to load nested references.
        /// </summary>
        /// <param name="db">Access to database connection instance</param>
        /// <param name="batch">A list of entries in loaded batch</param>
        protected virtual Task OnBatchLoaded(IDbConnection db, List<T> batch) {
            return Task.CompletedTask;
        }

        #endregion

    }

}
