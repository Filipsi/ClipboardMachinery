using System;
using System.Collections;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ClipboardMachinery.Core.Repositories.Lazy {

    public class LazyDataProvider<T> : ILazyDataProvider {

        private readonly DataRepository dataRepository;
        private readonly int batchSize;
        private int offset = 0;

        internal LazyDataProvider(DataRepository dataRepository, int batchSize) {
            this.dataRepository = dataRepository;
            this.batchSize = batchSize;
        }

        public IEnumerable<M> GetNextBatch<M>() {
            List<T> entries = dataRepository.Connection.Select<T>(
                "LIMIT @limit OFFSET @offset",
                new { limit = batchSize, offset }
            );

            offset += entries.Count;
            return dataRepository.Mapper.Map<M[]>(entries);
        }

    }

}
