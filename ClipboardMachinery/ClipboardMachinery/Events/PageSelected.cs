using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClipboardMachinery.Models;

namespace ClipboardMachinery.Events {

    public class PageSelected {

        public PageNavigatorModel Navigator { get; }

        public object Source { get; }

        public PageSelected(object source, PageNavigatorModel navigator) {
            Source = source;
            Navigator = navigator;
        }

    }
}
