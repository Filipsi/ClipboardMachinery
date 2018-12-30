using System.Collections.Generic;
using ServiceStack.OrmLite;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.Repositories.Lazy {

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
            List<T> entries = await dataRepository.Connection.SelectAsync<T>(
                "ORDER BY id DESC LIMIT @limit OFFSET @offset",
                new { limit = batchSize, offset }
            );

            offset += entries.Count;
            return dataRepository.Mapper.Map<M[]>(entries);
        }

        #endregion

    }

}
