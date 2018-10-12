using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Events {

    public class SetViewFilter {

        public Predicate<object> Filter { get; }

        public SetViewFilter(Predicate<object> filter) {
            Filter = filter;
        }

    }

}
