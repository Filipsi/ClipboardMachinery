using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using ClipboardMachinery.Components.Shell;
using ClipboardMachinery.Plumbing;

namespace ClipboardMachinery {

    internal class Bootstrapper : BootstrapperBase {

        #region Fields

        private readonly IWindsorContainer container = new WindsorContainer();

        #endregion

        #region Bootstrapper

        public Bootstrapper()
            => Initialize();

        protected override void Configure() {
            container
                .Register(Component.For<IWindsorContainer>().Instance(container).LifestyleSingleton());

            container
                .Install(FromAssembly.This());

            base.Configure();
        }

        protected override object GetInstance(Type service, string key)
            => string.IsNullOrEmpty(key)
                ? container.Resolve(service)
                : container.Resolve(service, key);

        protected override IEnumerable<object> GetAllInstances(Type service) {
            yield return container.ResolveAll(service);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
            => DisplayRootViewFor<IShell>();

        protected override void OnExit(object sender, EventArgs e) {
            container.Dispose();
            base.OnExit(sender, e);
        }

        #endregion

    }

}

