using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.Repository.LazyProvider {

    public interface ILazyDataProvider {

        /// <summary>
        /// Query database for a batch of clips with internal offset to retrieve history items.
        /// After successful query moves offset counter by batch size.
        /// </summary>
        /// <typeparam name="M">Type of model that the query instance should be mapped to and returned back</typeparam>
        /// <returns>An enumerable of queried clips mapped to M model</returns>
        Task<IEnumerable<M>> GetNextBatchAsync<M>();

        /// <summary>
        /// Resets the offset counter to the beginning of the history.
        /// </summary>
        void Reset();

    };

}
