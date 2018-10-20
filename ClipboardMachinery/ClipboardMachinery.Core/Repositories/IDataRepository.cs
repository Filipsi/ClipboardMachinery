using ClipboardMachinery.Core.Repositories.Lazy;
using ClipboardMachinery.Core.Repositories.Shema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.Repositories {

    public interface IDataRepository : IDisposable {

        ILazyDataProvider CreateLazyClipProvider(int batchSize);

        Task DeleteClip(int id);

    }

}
