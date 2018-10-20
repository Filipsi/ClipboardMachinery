using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.Repositories.Lazy {

    public interface ILazyDataProvider {

        Task<IEnumerable<M>> GetNextBatchAsync<M>();

    };

}
