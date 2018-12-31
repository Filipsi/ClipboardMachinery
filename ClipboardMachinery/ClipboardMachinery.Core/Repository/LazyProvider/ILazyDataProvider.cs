using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.Repository.LazyProvider {

    public interface ILazyDataProvider {

        Task<IEnumerable<M>> GetNextBatchAsync<M>();

    };

}
