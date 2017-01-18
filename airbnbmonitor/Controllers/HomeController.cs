using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using DataMiner;
using airbnbmonitor.Code;

namespace airbnbmonitor.Controllers
{
    public class ContaclViewModel
    {
        public string UrlContent { get; set; }
    }

    class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.ClientCertificates.Add(new X509Certificate());
            return request;
        }
    }

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var monitoring = new Monitoring();
            ViewBag.Marks = monitoring.GetData();
            return View();
        }

        public ActionResult About()
        {
            //ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            /*ViewBag.Message = "Your contact page.";
            string url = "https://www.airbnb.com/rooms/13666788?check_in=2016-06-25&guests=1&check_out=2016-06-26";
            string result = "NO RESULT";*/

            /*ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12
                                                   | SecurityProtocolType.Ssl3;*/

            /*using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent: Other");
                result = client.DownloadString(url);
            }*/

            var viewModel = new ContaclViewModel() { UrlContent = "" };
            return View(viewModel);
        }

        /// <summary>
        /// Certificate validation callback.
        /// </summary>
        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            return true;

            // If the certificate is a valid, signed certificate, return true.
            if (error == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            Console.WriteLine("X509Certificate [{0}] Policy Error: '{1}'",
                cert.Subject,
                error.ToString());

            return false;
        }
    }
}