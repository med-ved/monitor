namespace airbnbmonitor.Utils
{
    using Ninject;
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Code.Interfaces;
    using Code;
    using DataMiner.Database;
    using DataMiner;
    using DataMiner.Logger;

    public class NinjectDependencyResolver : IDependencyResolver
    {
        private readonly IKernel kernel;

        public NinjectDependencyResolver(IKernel kernelParam)
        {
            kernel = kernelParam;
            AddBindings();

            ObjectCreator.Init(kernel);
        }

        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }

        public static void AddBindings(IKernel kr)
        {
            kr.Bind<ICacheService>().To<CacheService>();
            kr.Bind<IDatabase>().To<Database>();
            kr.Bind<ILogger>().To<Logger>();

            var cache = kr.Get<ApplicationCache>();
            Func<Code.Definitions.CachedFlats> cachedFlats = () => { return cache.GetCachedFlats(); };
            kr.Bind<Analytics>().ToMethod(ctx => new Analytics(cachedFlats()));
        }

        private void AddBindings()
        {
            AddBindings(kernel);
        }
    }
}