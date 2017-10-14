namespace airbnbmonitor
{
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using Ninject;
    using DataMiner.Database;

    public class MvcApplication : System.Web.HttpApplication
    {
        private static IKernel _kernel;
        private static DataMiner.Program _dataMiner;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            if (_kernel == null)
            {
                _kernel = DependencyResolver.Current.GetService<IKernel>();
            }

            if (_dataMiner == null)
            {
                _dataMiner = _kernel.Get<DataMiner.Program>();
                //_dataMiner.Run();
            }
        }
    }
}
