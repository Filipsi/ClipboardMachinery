﻿using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using ClipboardMachinery.Plumbing;
using ClipboardMachinery.Windows.Shell;

namespace ClipboardMachinery {

    internal class AppBootstrapper : BootstrapperBase {

        #region Fields

        private readonly IWindsorContainer container = new WindsorContainer();

        #endregion

        #region Bootstrapper

        public AppBootstrapper()
            => Initialize();

        protected override void Configure()
            => container.Install(FromAssembly.This());

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
