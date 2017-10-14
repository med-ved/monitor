namespace airbnbmonitor.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using DataMiner.Database;
    using airbnbmonitor.Code;
    using System.Web.Script.Serialization;
    using Ninject;

    public class HomeController : Controller
    {
        private readonly IDatabase _db;
        private readonly IKernel _kernel;

        public HomeController(IDatabase db, IKernel kernel)
        {
            _kernel = kernel;
            _db = db; 
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetData(Analytics.MonitoringDataType type)
        {
            var analytics = _kernel.Get<Analytics>();
            var data = analytics.GetData(type);

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var serializedResult = serializer.Serialize(data);

            return Content(serializedResult);
        }

        [HttpPost]
        public JsonResult GetFlatsPopupData(IEnumerable<long> flatsIds)
        {
            var analytics = _kernel.Get<Analytics>();

            var startOfThePeriod = new DateTime(2016, 10, 1);
            var endOfThePeriod = DateTime.Now;
            var data = analytics.GetDataForFlats(flatsIds, startOfThePeriod, endOfThePeriod);

            return Json(data);
        }

        [HttpPost]
        public JsonResult GetOneFlatData(long flatId)
        {
            var analytics = _kernel.Get<Analytics>();

            var startOfThePeriod = new DateTime(2016, 10, 1);
            var endOfThePeriod = DateTime.Now;

            var flat = _db.GetFullFlatInfo(flatId);
            var data = analytics.GetFlat(flat, startOfThePeriod, endOfThePeriod);

            return Json(data);
        }
    }
}