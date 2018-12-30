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
            bool hasHandle = instance.GetType().GetInterfaces().Any(
                x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandle<>)
            );

            if (hasHandle) {
                Kernel.Resolve<IEventAggregator>().SubscribeOnPublishedThread(instance);
            }
        }

    }

}
