using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Services.Logging.NLogIntegration;
using Castle.Windsor;

namespace ClipboardMachinery.Core.Plumbing.Installers {

    public class LoggingInstaller : IWindsorInstaller {

        public void Install(IWindsorContainer container, IConfigurationStore store) {
            container.AddFacility<LoggingFacility>(
                f => f.LogUsing<NLogFactory>().WithConfig("nlog.config")
            );
        }

    }

}
