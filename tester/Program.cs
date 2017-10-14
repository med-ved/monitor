namespace tester
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using airbnbmonitor.Utils;
    using DataMiner;
    using DataMiner.Database;
    using DataMiner.UrlLoader;

    internal class Program
    {
        private static IDatabase _db;

        internal static void Main(string[] args)
        {
            ObjectCreator.Init(NinjectDependencyResolver.AddBindings);
            _db = ObjectCreator.Get<IDatabase>();

            var url = "https://www.airbnb.ru";
            var proxies = _db.GetProxies();
            int count = 0, succeses = 0, fails = 0;
            var tasks = new List<Thread>();
            foreach (var p in proxies)
            {
                 var t = new Thread(() => 
                 {
                    try
                    {
                        count++;
                        Console.WriteLine("Loading proxie {0}", count);
                        var loader = ObjectCreator.Get<LoaderWithTimeout>();
                        var result = loader.LoadUrl(url, p);
                        if (result.Error != null)
                        {
                            throw result.Error;
                        }

                        Thread.MemoryBarrier();
                        succeses++;
                    }
                    catch (Exception e)
                    {
                        Thread.MemoryBarrier();
                        fails++;
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Proxie failed, fails {0}", fails);
                    }
                });

                t.Start();
                tasks.Add(t);
            }

            foreach (var t in tasks)
            {
                t.Join();
            }

            Console.WriteLine(string.Format("count={0} successes={1} fails={2}", count, succeses, fails));
            Console.ReadLine();

            /*
             string[] lines = data.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
             int success = 0, failed = 0, skipped = 0;
             foreach(var line in lines)
             {
                 string url= "";
                 int port = 0;
                 try
                 {
                     var parts = line.Split(':');
                     url = parts[0];
                     port = Convert.ToInt32(parts[1]);
                     if (port != 80 && port != 8080)
                     {
                         Console.WriteLine("SKIPPING " + url + ":" + port.ToString());
                         skipped++;
                         continue;
                     }

                     using (var context = new MonitorEntities())
                     {
                         var newProxy = new Proxyes()
                         {
                             Id = Guid.NewGuid(),
                             Url = url,
                             Port = port,
                             Status = 0,
                             Ping = -1, 
                             LastUsageTime = null,
                             UseCount = 0,
                             Successes = 0,
                             Fails = 0,
                             Timeouts = 0
                         };

                         context.Proxyes.Add(newProxy);
                         context.SaveChanges();

                         success++;
                         Console.WriteLine("=> Added " + url + ":" + port.ToString());
                     }
                 }
                 catch(Exception e)
                 {
                     Console.WriteLine("ERROR " + url + ":" + port.ToString());
                     failed++;
                 }

             }

             Console.WriteLine("===> Added:" + success.ToString() + " falied:" + failed.ToString() + " skipped:" + skipped.ToString());
 */
            /*try
            {
                using (var client = new WebClient())
                {
                    var proxy = new WebProxy("125.16.240.197", 8080);
                    client.Proxy = proxy;
                    client.Encoding = Encoding.UTF8;
                    client.Headers.Add("User-Agent: Other");
                    var html = client.DownloadString("https://www.airbnb.ru/rooms/10206147");
                    Console.WriteLine(html);
                }

            }
            catch (WebException e)
            {
                Console.WriteLine(e);
            }


            Console.ReadLine();*/
        }
    }
}
