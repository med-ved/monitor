using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataMiner
{
    static class Loader
    {
        public static string Load(string url)
        {
            string html = null;

            bool done = false;
            while(!done)
            {
                
                try
                {
                    using (var client = new WebClient())
                    {
                        //var wp = new WebProxy("119.6.136.122:80");
                        //client.Proxy = wp;
                        client.Encoding = Encoding.UTF8;
                        client.Headers.Add("User-Agent: Other");
                        html = client.DownloadString(url);
                    }

                    done = true;
                }
                catch (WebException e)
                {
                    Logger.LogError(e, url);
                    Thread.Sleep(60000*30);
                }
            }

            return html;
        }
    }
}
