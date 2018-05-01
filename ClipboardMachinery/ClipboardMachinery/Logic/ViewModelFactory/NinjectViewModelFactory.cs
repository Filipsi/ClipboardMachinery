using Caliburn.Micro;
using Ninject;

namespace ClipboardMachinery.Logic.ViewModelFactory {

    internal class NinjectViewModelFactory : IViewModelFactory {

        private readonly IKernel _kernel;

        public NinjectViewModelFactory(IKernel kernel) {
            _kernel = kernel;
        }

        public T Create<T>() where T : IScreen {
            return _kernel.Get<T>();
        }

    }

}
