namespace airbnbmonitor
{
    using System.Web.Optimization;
    using System.Web.Optimization.React;

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

            bundles.Add(new BabelBundle("~/bundles/jsx").Include(
                // Add your JSX files here
                "~/Scripts/actions.jsx",
                "~/Scripts/common.jsx",

                "~/Scripts/charts/donutChart.jsx",
                "~/Scripts/charts/lineChart.jsx",

                "~/Scripts/components/chartsPanel.jsx",
                "~/Scripts/components/component.jsx",
                "~/Scripts/components/container.jsx",
                "~/Scripts/components/flatsPopup.jsx",
                "~/Scripts/components/header.jsx",
                "~/Scripts/components/headerButton.jsx",
                "~/Scripts/components/loadingAnimation.jsx",
                "~/Scripts/components/map.jsx",
                "~/Scripts/components/oneFlatDetailsPopup.jsx",
                "~/Scripts/components/overlay.jsx",
                "~/Scripts/components/popup.jsx",
                "~/Scripts/components/textGrid.jsx",
                "~/Scripts/components/textGridItem.jsx",
                "~/Scripts/components/widget.jsx",

                "~/Scripts/stores/baseStore.jsx",
                "~/Scripts/stores/dataStore.jsx",
                "~/Scripts/stores/flatsPopupDataStore.jsx",
                "~/Scripts/stores/oneFlatDetailsStore.jsx",
                
                "~/Scripts/app.jsx"
            ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css"));
        }
    }
}
