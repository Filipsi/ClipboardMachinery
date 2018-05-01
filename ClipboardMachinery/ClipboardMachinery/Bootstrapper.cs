using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using ClipboardMachinery.Logic;
using ClipboardMachinery.Logic.ClipboardItemsProvider;
using ClipboardMachinery.Logic.HotKeyHandler;
using ClipboardMachinery.Logic.ViewModelFactory;
using ClipboardMachinery.ViewModels;
using Ninject;

namespace ClipboardMachinery {

    internal class Bootstrapper : BootstrapperBase {

        private readonly IKernel _kernel = new StandardKernel();

        public Bootstrapper() => Initialize();

        protected override void Configure() {
            _kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();
            _kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            _kernel.Bind<IShell>().To<ShellViewModel>().InSingletonScope();
            _kernel.Bind<IViewModelFactory>().To<NinjectViewModelFactory>().InSingletonScope();
            _kernel.Bind<IClipboardItemsProvider>().To<ClipboardItemsProviderImpl>().InSingletonScope();
            _kernel.Bind<IHotKeyHandler>().To<HotKeyHandlerImpl>().InSingletonScope();

            base.Configure();
        }

        protected override object GetInstance(Type service, string key) {
            return string.IsNullOrEmpty(key) ? _kernel.Get(service) : _kernel.Get(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service) {
            return _kernel.GetAll(service);
        }

        protected override void BuildUp(object instance) {
            _kernel.Inject(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e) {
            DisplayRootViewFor<IShell>();
        }

        protected override void OnExit(object sender, EventArgs e) {
            base.OnExit(sender, e);
            _kernel.Dispose();
        }

    }

}

