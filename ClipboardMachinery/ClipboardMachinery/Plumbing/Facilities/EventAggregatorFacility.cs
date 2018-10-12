using Caliburn.Micro;
using Castle.MicroKernel.Facilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Plumbing.Facilities {

    public class EventAggregatorFacility : AbstractFacility {

        protected override void Init() {
            Kernel.ComponentCreated += OnComponentCreated;
        }

        private void OnComponentCreated(Castle.Core.ComponentModel model, object instance) {
            if (typeof(IHandle).IsAssignableFrom(instance.GetType())) {
                Kernel.Resolve<IEventAggregator>().Subscribe(instance as IHandle);
            }
        }

    }

}
