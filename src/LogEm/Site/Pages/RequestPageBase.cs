using System;
using System.Web.UI;
using LogEm.Logging.RequestLogs;
using LogEm.Utilities;

using CultureInfo = System.Globalization.CultureInfo;

namespace LogEm
{
    /// <summary>
    /// Provides the base implementation and layout for most pages that render 
    /// HTML for the error log.
    /// </summary>
    internal abstract class RequestPageBase : Page
    {
        private string _title;
        private RequestLog _log;

        protected string BasePageName
        {
            get { return this.Request.ServerVariables["URL"]; }
        }

        protected virtual RequestLog RequestLog
        {
            get
            {
                if (_log == null)
                    _log = RequestLog.GetLog(Context);

                return _log;
            }
        }

        protected virtual string PageTitle
        {
            get { return StringUtils.IfNull(_title); }
            set { _title = value; }
        }

        protected virtual string ApplicationName
        {
            get { return this.RequestLog.ApplicationName; }
        }

        protected virtual void RenderDocumentStart(HtmlTextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            writer.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");

            writer.AddAttribute("xmlns", "http://www.w3.org/1999/xhtml");
            writer.RenderBeginTag(HtmlTextWriterTag.Html);  // <html>

            writer.RenderBeginTag(HtmlTextWriterTag.Head);  // <head>
            RenderHead(writer);
            writer.RenderEndTag();                          // </head>
            writer.WriteLine();

            writer.RenderBeginTag(HtmlTextWriterTag.Body);  // <body>
        }

        protected virtual void RenderHead(HtmlTextWriter writer)
        {
            //
            // In IE 8 or later, mimic IE 7
            // http://msdn.microsoft.com/en-us/library/cc288325.aspx#DCModes
            //

            writer.AddAttribute("http-equiv", "X-UA-Compatible");
            writer.AddAttribute("content", "IE=EmulateIE7");
            writer.RenderBeginTag(HtmlTextWriterTag.Meta);
            writer.RenderEndTag();
            writer.WriteLine();

            //
            // Write the document title.
            //

            writer.RenderBeginTag(HtmlTextWriterTag.Title);
            Server.HtmlEncode(this.PageTitle, writer);
            writer.RenderEndTag();
            writer.WriteLine();

            //
            // Write a <link> tag to relate the style sheet.
            //

#if NET_1_0 || NET_1_1
            writer.AddAttribute("rel", "stylesheet");
#else
            writer.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
#endif
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
            writer.AddAttribute(HtmlTextWriterAttribute.Href, this.BasePageName + "/stylesheet");
            writer.RenderBeginTag(HtmlTextWriterTag.Link);
            writer.RenderEndTag();
            writer.WriteLine();
        }

        protected virtual void RenderDocumentEnd(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, "Footer");
            writer.RenderBeginTag(HtmlTextWriterTag.P); // <p>

            //
            // Write the powered-by signature, that includes version information.
            //

            //PoweredBy poweredBy = new PoweredBy();
            //poweredBy.RenderControl(writer);

            //
            // Write out server date, time and time zone details.
            //

            DateTime now = DateTime.Now;

            writer.Write("Server date is ");
            this.Server.HtmlEncode(now.ToString("D", CultureInfo.InvariantCulture), writer);

            writer.Write(". Server time is ");
            this.Server.HtmlEncode(now.ToString("T", CultureInfo.InvariantCulture), writer);

            writer.Write(". All dates and times displayed are in the ");
            writer.Write(TimeZone.CurrentTimeZone.IsDaylightSavingTime(now) ?
                TimeZone.CurrentTimeZone.DaylightName : TimeZone.CurrentTimeZone.StandardName);
            writer.Write(" zone. ");

            //
            // Write out the source of the log.
            //

            writer.Write("This log is provided by the ");
            this.Server.HtmlEncode(this.RequestLog.Name, writer);
            writer.Write('.');

            writer.RenderEndTag(); // </p>

            writer.RenderEndTag(); // </body>
            writer.WriteLine();

            writer.RenderEndTag(); // </html>
            writer.WriteLine();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            RenderDocumentStart(writer);
            RenderContents(writer);
            RenderDocumentEnd(writer);
        }

        protected virtual void RenderContents(HtmlTextWriter writer)
        {
            base.Render(writer);
        }
    }
}
