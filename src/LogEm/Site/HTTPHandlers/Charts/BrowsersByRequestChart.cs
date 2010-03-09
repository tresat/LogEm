﻿using System;
using System.IO;
using System.Web;
using System.Web.UI.DataVisualization.Charting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogEm.Logging.RequestLogs;

namespace LogEm.Site.HTTPHandlers.Charts
{
    internal class BrowsersByRequestChart : ChartHttpHandlerBase
    {
        public override void ProcessRequest(HttpContext context) {
            Chart browsersChart = new Chart();
            browsersChart.Titles.Add("Requests by Browser");
            browsersChart.Height = 500;
            browsersChart.Width = 500;

            // Create new chart area
            ChartArea area = new ChartArea("Browsers");

            Series series = RequestLogBase.GetLog(HttpContext.Current).SeriesBrowsersByRequest();

            browsersChart.ChartAreas.Add(area);
            browsersChart.Series.Add(series);

            // Simply use a MemoryStream to save the chart. 
            MemoryStream imageStream = new MemoryStream();
            browsersChart.SaveImage(imageStream, ChartImageFormat.Png);

            // Reset the stream’s pointer back to the start of the stream. 
            imageStream.Seek(0, SeekOrigin.Begin);

            // And render the stream to the response
            context.Response.Clear();
            using (BinaryWriter writer = new BinaryWriter(context.Response.OutputStream)) {
                writer.Write(imageStream.GetBuffer());
            }
            context.Response.End();
        }
    }
}