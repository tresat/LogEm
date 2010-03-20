using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.DataVisualization.Charting;
using System.IO;
using LogEm.Utilities;

namespace LogEm.Site.Pages
{
    public class BrowserInfoPage : LogEmPageBase
    {
        public BrowserInfoPage(): base()
        {
            _head.Controls.Add(HtmlUtils.CreateLinkToJavaScriptFile("http://www.google.com/jsapi", false));
            _head.Controls.Add(HtmlUtils.CreateLinkToJavaScriptFile("BrowserInfo.js", true));
        }

        protected override void RenderContents()
        {
            HtmlGenericControl contents = new HtmlGenericControl("div");
            contents.ID = "contents";

            HtmlGenericControl h1 = new HtmlGenericControl("h1");
            h1.InnerText = "Browser Information";
            contents.Controls.Add(h1);

            HtmlGenericControl timeline = new HtmlGenericControl("div");
            timeline.ID = "timeline";
            timeline.Attributes.Add("style", "width: 800px; height: 400px;");
            contents.Controls.Add(timeline);

            HtmlImage chart = new HtmlImage();
            chart.ID = "chartBrowsersByRequest";
            chart.Src = HtmlUtils.BaseLogEmUrl() + "chart?type=browsers-by-request";
            chart.Height = 500;
            chart.Width = 500;

            contents.Controls.Add(chart);

            _body.Controls.Add(contents);
        }
    }
}
