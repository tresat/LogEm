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
        /// <summary>
        /// Writes the HTML Head element.  Adds site stylesheet.
        /// </summary>
        protected override void RenderHtmlHead()
        {
            _head = new HtmlHead();

            HtmlTitle title = new HtmlTitle();
            title.Text = _title;
            _head.Controls.Add(title);

            HtmlLink logEmCSS = new HtmlLink();
            logEmCSS.Attributes.Add("rel", "stylesheet");
            logEmCSS.Attributes.Add("type", "text/css");
            logEmCSS.Attributes.Add("href", HtmlUtils.BaseLogEmUrl() + "stylesheet");
            _head.Controls.Add(logEmCSS);

            HtmlGenericControl logEmJS = new HtmlGenericControl("script");
            logEmJS.Attributes.Add("type", "text/javascript");
            logEmJS.Attributes.Add("src", HtmlUtils.BaseLogEmUrl() + "javascript");
            _head.Controls.Add(logEmJS);

            Page.Controls.Add(_head);

            _head.RenderControl(_writer);
            _writer.WriteLine();
        }

        protected override void RenderContents()
        {
            HtmlGenericControl contents = new HtmlGenericControl("div");
            contents.ID = "contents";

            HtmlGenericControl h1 = new HtmlGenericControl("h1");
            h1.InnerText = "Browser Information";
            contents.Controls.Add(h1);

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
