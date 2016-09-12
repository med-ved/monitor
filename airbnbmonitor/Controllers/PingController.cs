using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace airbnbmonitor.Controllers
{
    public class PingController : Controller
    {
        // GET: Ping
        [AllowAnonymous]
        public ActionResult Index()
        {
            DataMiner.Logger.WriteLine("ping","Pong");
            return View();
        }
    }
}