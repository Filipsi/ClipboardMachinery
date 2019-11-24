using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Castle.Core.Logging;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using ClipboardMachinery.Plumbing.Facilities;

namespace ClipboardMachinery.Plumbing {

    internal class AppBootstrapper : BootstrapperBase {

        #region Fields

        public const string REPOSITORY_OWNER = "Filipsi";
        public const string REPOSITORY_NAME = "ClipboardMachinery";

        private readonly IWindsorContainer container = new WindsorContainer();
        private ILogger logger;

        #endregion

        #region Bootstrapper

        public AppBootstrapper() {
            Initialize();
        }

        protected override void Configure() {
            container
                .Kernel
                .Resolver
                .AddSubResolver(new CollectionResolver(container.Kernel));

            container
                .AddFacility<TypedFactoryFacility>()
                .AddFacility<EventAggregatorFacility>()
                .Install(FromAssembly.Named("ClipboardMachinery.Core"))
                .Install(FromAssembly.This());

            logger = container.Resolve<ILogger>();
        }

        protected override object GetInstance(Type service, string key) {
            return string.IsNullOrEmpty(key) ? container.Resolve(service) : container.Resolve(key, service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service) {
            yield return container.ResolveAll(service);
        }

        protected override async void OnStartup(object sender, StartupEventArgs e) {
            await DisplayRootViewForAsync(typeof(IShell));
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            logger.Fatal("Encountered fatal error!", e.Exception);

            // Gracefully shut down the application
            // This is needed to remove pinvoke hooks
            e.Handled = true;
            Application.Shutdown();
        }

        protected override void OnExit(object sender, EventArgs e) {
            container.Dispose();
            base.OnExit(sender, e);
        }

        #endregion

    }

}

