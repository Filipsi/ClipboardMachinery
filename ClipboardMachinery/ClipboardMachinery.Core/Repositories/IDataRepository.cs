using ClipboardMachinery.Core.Repositories.Lazy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.Repositories {

    public interface IDataRepository : IDisposable {

        ILazyDataProvider CreateLazyClipProvider(int batchSize);

        Task InsertClip(string content, DateTime created, KeyValuePair<string, string>[] tags = null);

        Task DeleteClip(int id);

    }

}
