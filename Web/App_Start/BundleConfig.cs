using System.Configuration;
using System.Web;
using System.Web.Optimization;
using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.Transformers;

namespace Compilify.Web
{
    public class BundleConfig
    {
        private static readonly IBundleOrderer DefaultBundleOrderer = new NullOrderer();

        public static void RegisterBundles(BundleCollection bundles)
        {
            RegisterStyleSheets(bundles);
            RegisterScripts(bundles);
        }

        private static void RegisterStyleSheets(BundleCollection bundles)
        {
            var vendorCss = new StyleBundle("~/styles/libs") { Orderer = DefaultBundleOrderer };
            vendorCss.Include(
                "~/assets/css/vendor/jquery.ui.core.css",
                "~/assets/css/vendor/jquery.ui.resizable.css",
                "~/assets/css/vendor/bootstrap.css",
                "~/assets/css/vendor/font-awesome.css",
                "~/assets/css/vendor/codemirror.css",
                "~/assets/css/vendor/codemirror.neat.css");
            bundles.Add(vendorCss);

            var less = new StyleBundle("~/styles/app") { Orderer = DefaultBundleOrderer };
            less.Transforms.Add(new CssTransformer());
            less.Include("~/assets/less/compilify.less");
            bundles.Add(less);
        }

        private static void RegisterScripts(BundleCollection bundles)
        {
            var transform = new JsTransformer();

            var vendorjs = new ScriptBundle("~/js/libs") { Orderer = DefaultBundleOrderer };
            vendorjs.Transforms.Add(transform);
            vendorjs.Include("~/assets/js/vendor/json2.js");

            vendorjs.Include(
                "~/assets/js/vendor/jquery.js",
                "~/assets/js/vendor/jquery.ui.core.js",
                "~/assets/js/vendor/jquery.ui.widget.js",
                "~/assets/js/vendor/jquery.ui.mouse.js",
                "~/assets/js/vendor/jquery.ui.resizable.js",
                "~/assets/js/vendor/jquery.signalr.js");

            vendorjs.Include(
                "~/assets/js/vendor/underscore.js");

            vendorjs.Include(
                "~/assets/js/vendor/doT.js");

            vendorjs.Include(
                // "~/assets/js/vendor/bootstrap.js",
                "~/assets/js/vendor/codemirror.js",
                "~/assets/js/vendor/codemirror.clike.js",
                "~/assets/js/vendor/shortcut.js");

            vendorjs.ConcatenationToken = ";";
            bundles.Add(vendorjs);

            var js = new ScriptBundle("~/js/app") { Orderer = DefaultBundleOrderer };
            js.Transforms.Add(transform);
            js.Include("~/assets/js/Events.js");
            js.Include("~/assets/js/Document.js");
            js.Include("~/assets/js/DocumentManager.js");
            js.Include("~/assets/js/DocumentTabsView.js");
            js.Include("~/assets/js/Editor.js");
            js.Include("~/assets/js/EditorManager.js");
            js.Include("~/assets/js/ProjectManager.js");
            js.Include("~/assets/js/ReferenceManager.js");
            js.Include("~/assets/js/Sidebar.js");
            js.Include("~/assets/js/TemplateManager.js");
            js.Include("~/assets/js/app.js");
            bundles.Add(js);
        }
    }
}
