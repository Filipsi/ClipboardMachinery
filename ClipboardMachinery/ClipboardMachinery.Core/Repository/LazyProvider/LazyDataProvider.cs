using System.Collections.Generic;
using ServiceStack.OrmLite;
using System.Threading.Tasks;
using System;
using ServiceStack.OrmLite.Legacy;
using System.Data;
using ClipboardMachinery.Core.Repository.Schema;

namespace ClipboardMachinery.Core.Repository.LazyProvider {

    public class LazyDataProvider<T> : ILazyDataProvider {

        #region Fields

        private readonly DataRepository dataRepository;
        private readonly int batchSize;
        private int offset = 0;

        #endregion

        internal LazyDataProvider(DataRepository dataRepository, int batchSize) {
            this.dataRepository = dataRepository;
            this.batchSize = batchSize;
        }

        #region Logic

        public async Task<IEnumerable<M>> GetNextBatchAsync<M>() {
            IDbConnection db = dataRepository.Connection;

            // Create SQL query
            SqlExpression<T> query = db.From<T>().OrderByDescending("id").Limit(batchSize);
            query.Offset = offset;

            // Load entries of type T
            List<T> entries = await db.LoadSelectAsync(query);

            // HACK: Load nested references for clip tags
            if (typeof(T) == typeof(Clip)) {
                foreach (Clip clip in entries as List<Clip>) {
                    if (clip.Tags != null) {
                        foreach (Tag tag in clip.Tags) {
                            await db.LoadReferencesAsync(tag);
                        }
                    }
                }
            }

            // Move offset of lazy loader
            offset += entries.Count;

            // Map T results to desired models
            return dataRepository.Mapper.Map<M[]>(entries);
        }

        #endregion

    }

}
