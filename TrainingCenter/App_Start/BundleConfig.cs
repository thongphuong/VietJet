using System.Web;
using System.Web.Optimization;

namespace TrainingCenter
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/corejs").Include(
                "~/Content/assets/js/jquery-2.2.4.min.js",
                "~/Content/assets/js/metisMenu.min.js",
                "~/Content/assets/js/scripts/init.js",
                "~/Content/assets/js/jquery.counterup.min.js",
                "~/Content/assets/js/ripple.min.js",
                "~/Content/assets/js/jquery.slimscroll.min.js",
                "~/Content/assets/js/sweetalert.min.js",
                "~/Content/assets/js/datatables.min.js",
                "~/Content/assets/js/dataTables.responsive.js",
                "~/Content/assets/js/dataTables.buttons.min.js",
                "~/Content/Js/jquery-ui/jquery-ui.min.js",
                "~/Content/Js/jstree/js/jquery.tree.min.js",
                "~/Content/Js/select2/js/select2.full.min.js",
                "~/Content/Js/animateNumber/jquery.animateNumber.js",
                "~/Scripts/js/bootstrap.min.js",
                "~/Scripts/js/common.js",
                "~/Scripts/moment.min.js",
                "~/Scripts/moment-with-locales.min.js",
                "~/Scripts/bootstrap-datetimepicker.min.js",
                "~/Scripts/ScriptSys/ScriptTool/ToolFunction.js",
                "~/Scripts/ScriptSys/ScriptTool/ToolTMS.js",
                "~/Scripts/ScriptSys/ScriptTMS/Shared/_layout.js",
                "~/Scripts/js/plugins/timepicker/bootstrap-timepicker.min.js",
                "~/Scripts/js/plugins/daterangepicker/daterangepicker.js",
                "~/Content/assets/js/charts/morris/morris.min.js",
                "~/Content/assets/js/raphael/raphael.min.js",
                "~/Content/assets/js/calendar/moment-with-locales.js",
                "~/Content/assets/js/calendar/fullcalendar.min.js",
                "~/Content/assets/js/calendar/bootstrap-material-datetimepicker.js",
                "~/Content/assets/js/calendar/bootstrap-datetimepicker.min.js",
                "~/Content/assets/js/scripts/calendar.js",
                "~/Content/Js/Datatable/Colspan.js",
                "~/Content/interact.js",
                "~/Content/assets/js/shieldui-all.min.js",
                "~/Content/fontawesome-free-5.3.1-web/js/all.js",
                "~/Content/Highcharts/code/highcharts.js"

                ));
            bundles.Add(new StyleBundle("~/bundles/corecss").Include(
                                    "~/Content/assets/css/bootstrap.css",
                                    "~/Content/assets/css/metisMenu.min.css",
                                    "~/Content/assets/css/material-design-iconic-font.min.css",
                                    "~/Content/assets/css/animate.min.css",
                                    "~/Content/assets/css/ripple.min.css",
                                    "~/Content/assets/css/hover.css",
                                    "~/Content/assets/css/sweetalert.min.css",
                                    "~/Content/assets/css/gallery/lightgallery.min.css",
                                    "~/Content/assets/css/gallery/justifiedGallery.min.css",
                                    "~/Content/assets/css/jquery-jvectormap.css",
                                    "~/Content/assets/css/calendar/fullcalendar.min.css",
                                    "~/Content/assets/css/metisMenu.min.css",
                                    "~/Content/assets/css/material-design-iconic-font.min.css",
                                    "~/Content/assets/css/font-awesome.css",
                                    "~/Content/assets/css/deluxe-admin.css",
                                    "~/Content/assets/css/datatables.min.css",
                                    "~/Content/assets/css/vietjet.css",
                                    "~/Content/assets/css/calendar/bootstrap-datetimepicker.min.css",
                                    "~/Content/Js/jstree/css/jquery.tree.css",
                                    "~/Content/Js/jstree/css/Customize.css",
                                    "~/Content/Js/jstree/themes/default/style.min.css",
                                    "~/Content/Js/bootrap-select/bootstrap-select.min.css",
                                    "~/Content/Js/select2/css/select2.min.css",
                                    "~/Content/css/daterangepicker/daterangepicker-bs3.css",
                                    "~/Content/css/jQueryUI/jquery-ui.css",
                                    "~/Content/css/VietJetFont.css",
                                    "~/Content/app.css",
                                    "~/Content/assets/css/iconloading.css",
                                    "~/Content/sweetalert/sweetalert.css",
                                    "~/Content/css/timepicker/bootstrap-timepicker.min.css",
                                    "~/Content/assets/css/all.min.css",
                                    "~/Content/fontawesome-free-5.3.1-web/css/all.css",
                                    "~" +System.Configuration.ConfigurationManager.AppSettings["PrivateStyle"].ToString()
                ));

            BundleTable.EnableOptimizations = true;

        }
    }
}