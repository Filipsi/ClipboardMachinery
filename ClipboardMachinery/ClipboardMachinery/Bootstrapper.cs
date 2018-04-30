using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using ClipboardMachinery.ViewModels;

namespace ClipboardMachinery {

    internal class Bootstrapper : BootstrapperBase {

        private readonly SimpleContainer _container = new SimpleContainer();

        public Bootstrapper() => Initialize();

        protected override void Configure() {
            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.PerRequest<IShell, ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key) {
            object instance = _container.GetInstance(service, key);

            if (instance == null)
                throw new InvalidOperationException($"Could not locate any instances of type {service} with key {key}.");

            return instance;
        }

        protected override IEnumerable<object> GetAllInstances(Type service) {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance) {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e) {
            DisplayRootViewFor<IShell>();
        }

    }

}

