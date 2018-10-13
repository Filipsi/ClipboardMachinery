using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.Repositories.Lazy {

    public interface ILazyDataProvider {

        IEnumerable<M> GetNextBatch<M>();

    };

}
