namespace airbnbmonitor
{
    using System.Web.Optimization;

    public class BundleConfig
    {
        //Дополнительные сведения об объединении см. по адресу: http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                        "~/Scripts/libs/jquery-3.2.1.min.js",
                        "~/Scripts/libs/markerclusterer.js",
                        "~/Scripts/libs/heatmap.js",
                        "~/Scripts/libs/gmaps-heatmap.js",

                        "~/Scripts/libs/react.min.js",
                        "~/Scripts/libs/react-dom.min.js",
                        "~/Scripts/libs/remarkable.min.js",
                        "~/Scripts/libs/highcharts.js",

                        "~/Scripts/libs/moment.min.js",
                        "~/Scripts/libs/reflux.min.js",

                        "~/Scripts/objectAssign.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css"));
        }
    }
}
