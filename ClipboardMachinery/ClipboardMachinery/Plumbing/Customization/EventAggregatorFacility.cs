using Caliburn.Micro;
using Castle.Core;
using Castle.MicroKernel.Facilities;
using System.Linq;

namespace ClipboardMachinery.Plumbing.Customization {

    public class EventAggregatorFacility : AbstractFacility {

        #region Facility

        protected override void Init() {
            Kernel.ComponentCreated += OnComponentCreated;
            Kernel.ComponentDestroyed += OnComponentDestroyed;
        }

        protected override void Dispose() {
            Kernel.ComponentCreated -= OnComponentCreated;
            Kernel.ComponentDestroyed -= OnComponentDestroyed;
            base.Dispose();
        }

        #endregion

        #region Handlers

        private void OnComponentCreated(ComponentModel model, object instance) {
            if (HasHandle(instance)) {
                Kernel.Resolve<IEventAggregator>().SubscribeOnPublishedThread(instance);
            }
        }

        private void OnComponentDestroyed(ComponentModel model, object instance) {
            if (HasHandle(instance)) {
                Kernel.Resolve<IEventAggregator>().Unsubscribe(instance);
            }
        }

        #endregion

        #region Helpers

        private bool HasHandle(object instance) {
            return instance.GetType().GetInterfaces().Any(
                x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandle<>)
            );
        }

        #endregion

    }

}
