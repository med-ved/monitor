namespace airbnbmonitor.Controllers
{
    using System.Web.Mvc;
    using airbnbmonitor.Code;
    using DataMiner.Logger;
    using Ninject;

    public class PingController : Controller
    {
        private readonly IKernel _kernel;
        private readonly ILogger _log;

        public PingController(IKernel kernel, ILogger log)
        {
            _kernel = kernel;
            _log = log;
        }

        // GET: Ping
        [AllowAnonymous]
        public ActionResult Index()
        {
            _log.WriteLine("ping", "Pong");

            var cache = _kernel.Get<ApplicationCache>();
            cache.GetCachedFlats();

            return View();
        }
    }
}