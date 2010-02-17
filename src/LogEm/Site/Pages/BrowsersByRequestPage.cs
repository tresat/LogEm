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

namespace LogEm.Site.Pages
{
    public class BrowsersByRequestPage : LogEmPageBase
    {
        protected override void RenderContents()
        {
            HtmlGenericControl contents = new HtmlGenericControl("div");
            contents.ID = "contents";

            HtmlGenericControl h1 = new HtmlGenericControl("h1");
            h1.InnerText = "Requests by Browser Type";
            contents.Controls.Add(h1);

            Chart browsersChart = new Chart();
            browsersChart.ImageStorageMode = ImageStorageMode.UseHttpHandler;
            browsersChart.Width = 500;
            browsersChart.Height = 500;
            browsersChart.Titles.Add("Requests by Browser Type");

            //contents.Controls.Add(browsersChart);

            // This is the most important part, and the departure from using any custom classes or Futures library.

            //// Simply use a MemoryStream to save the chart. 
            //MemoryStream imageStream = new MemoryStream();
            //browsersChart.SaveImage(imageStream, ChartImageFormat.Png);

            //// Reset the stream’s pointer back to the start of the stream. 
            //imageStream.Seek(0, SeekOrigin.Begin);

            // return the normal FileResult available in the current release of MVC

            browsersChart.RenderType = RenderType.ImageTag;
            browsersChart.ImageLocation = "~/MyChart.png";

            browsersChart.SaveImage(Server.MapPath("~/MyChart.png"), ChartImageFormat.Png);

            HtmlImage chart = new HtmlImage();
            chart.ID = "browsers chart";
            chart.Src = Request.ApplicationPath + "/MyChart.png";
            chart.Height = 500;
            chart.Width = 500;

            contents.Controls.Add(chart);

            //contents.Controls.Add(browsersChart);

            //browsersChart.SaveImage(Response.OutputStream, ChartImageFormat.Png);
            //Response.End();

            _body.Controls.Add(contents);
        }
    }
}
