using System;
using System.Collections;
using System.Collections.Generic;

namespace ClipboardMachinery.Common.Helpers {

    public class ComparisonComparer<T> : IComparer<T>, IComparer {

        private readonly Comparison<T> comparison;

        public ComparisonComparer(Comparison<T> comparison) {
            this.comparison = comparison;
        }

        public int Compare(T x, T y) {
            return comparison(x, y);
        }

        public int Compare(object o1, object o2) {
            return comparison((T)o1, (T)o2);
        }

    }

}
