using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.DataStorage {

    public interface ILazyDataProvider {

        /// <summary>
        /// Type of a data model this provider is internally using.
        /// </summary>
        Type DataType { get; }

        /// <summary>
        /// Number of items that are provided when retrieving data.
        /// </summary>
        int BatchSize { get; }

        /// <summary>
        /// Offset counter pointing to the current point in history.
        /// </summary>
        int Offset { get; set; }

        /// <summary>
        /// Query database for a batch of entries with internal offset to retrieve history items.
        /// After successful query moves offset counter by batch size.
        /// </summary>
        /// <typeparam name="TM">Type of model that the query instance should be mapped to and returned back</typeparam>
        /// <returns>An enumerable of queried entries mapped to M model</returns>
        Task<IEnumerable<TM>> GetNextBatchAsync<TM>();

    }

}
