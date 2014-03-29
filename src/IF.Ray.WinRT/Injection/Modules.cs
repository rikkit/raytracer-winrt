using IF.Common.Metro.Progress;
using Ninject.Modules;
using Windows.UI.Core;

namespace IF.Ray.WinRT.Injection
{

    public class AppModule : NinjectModule
    {
        private readonly CoreDispatcher _dispatcher;

        public AppModule(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public override void Load()
        {
            Bind<CoreDispatcher>().ToConstant(_dispatcher);
            Bind<IProgressAggregator>().To<ProgressAggregator>();
        }
    }
}
