using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Events {

    internal class ChangeAppVisiblity {

        public VisiblityChangeType EventVisiblityChangeType { get; }

        public ChangeAppVisiblity(VisiblityChangeType visiblityChangeType) {
            EventVisiblityChangeType = visiblityChangeType;
        }
    }

    public enum VisiblityChangeType {
        Hide,
        Show,
        Toggle
    }

}
