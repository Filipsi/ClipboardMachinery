using Caliburn.Micro;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.ModelBuilder;
using System.Linq;

namespace ClipboardMachinery.Plumbing.Customization {

    public class DoNotWireActiveItem : IContributeComponentModelConstruction {

        public void ProcessModel(IKernel kernel, ComponentModel model) {
            if (!typeof(IHaveActiveItem).IsAssignableFrom(model.Implementation)) {
                return;
            }

            PropertySet activeItemProp = model.Properties.FirstOrDefault(
                info => info.Property.Name == nameof(IHaveActiveItem.ActiveItem)
            );

            if (activeItemProp == null) {
                return;
            }

            model.Dependencies.Remove(activeItemProp.Dependency);
        }

    }

}
