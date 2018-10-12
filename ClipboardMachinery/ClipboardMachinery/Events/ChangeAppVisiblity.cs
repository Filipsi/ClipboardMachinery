using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Events {

    public class ChangeAppVisiblity {

        public VisiblityChangeType ChangeType { get; }

        public ChangeAppVisiblity(VisiblityChangeType visiblityChangeType) {
            ChangeType = visiblityChangeType;
        }
    }

    public enum VisiblityChangeType {
        Hide,
        Show,
        Toggle
    }

}
