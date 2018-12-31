using ClipboardMachinery.Core.Repository.LazyProvider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.Repository {

    public interface IDataRepository : IDisposable {

        ILazyDataProvider CreateLazyClipProvider(int batchSize);

        Task InsertClip(string content, DateTime created, KeyValuePair<string, object>[] tags = null);

        Task DeleteClip(int id);

    }

}
