using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Plumbing {

    public interface IScreenPage : IScreen {

        string Title { get; }

        string Icon { get; }

    }

}
