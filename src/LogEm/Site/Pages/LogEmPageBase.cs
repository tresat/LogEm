using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using LogEm.Utilities;
using LogEm.Logging.RequestLogs;
using System.Globalization;
using System.Text;
using System.Reflection;

namespace LogEm.Site.Pages
{
    public class LogEmPageBase : Page
    {
#region Members
        protected RequestLog _log;
        protected HtmlTextWriter _writer;
        protected String _title;

        protected bool _showHeader;
        protected bool _showFooter;

        protected HtmlHead _head;
        protected HtmlGenericControl _body;
#endregion

#region Properties
        public virtual RequestLog RequestLog
        {
            get
            {
                // Save reference to log on first request
                if (_log == null)
                    _log = RequestLog.GetLog(Context);

                return _log;
            }
        }
#endregion

        #region Constructors
        /// <summary>
        /// Default constructor: titles page LogEm and adds header/footer sections.
        /// </summary>
        public LogEmPageBase() : base() {
            _title = "LogEm";
            _showHeader = true;
            _showFooter = true;
        } 
        #endregion

        #region Page Rendering Scaffolding Functions
        /// <summary>
        /// This is the starting function responsible for page rendering.
        /// </summary>
        /// <param name="writer">The writer creating the HTML for the page.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            _writer = writer;

            RenderDocumentStart();
            RenderHtmlHead();
            RenderHtmlBody();
            RenderDocumentEnd();
        }

        /// <summary>
        /// Creates the DOCTYPE start lines and opens the HTML tag.
        /// </summary>
        /// <param name="writer">The writer creating the HTML for the page.</param>
        protected virtual void RenderDocumentStart()
        {
            _writer.WriteLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN" + @"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" + "\">");
 
            _writer.AddAttribute("xmlns", "http://www.w3.org/1999/xhtml");
            _writer.RenderBeginTag(HtmlTextWriterTag.Html);
        }

        /// <summary>
        /// Ends the HTML tag.
        /// </summary>
        /// <param name="writer"></param>
        protected virtual void RenderDocumentEnd()
        {
            _writer.RenderEndTag(); // HTML
            _writer.WriteLine();
        }

        /// <summary>
        /// Writes the HTML Head element.  Adds site stylesheet.
        /// </summary>
        protected virtual void RenderHtmlHead()
        {
            _head = new HtmlHead();

            HtmlTitle title = new HtmlTitle();
            title.Text = _title;
            _head.Controls.Add(title);

            HtmlLink logEmCSS = new HtmlLink();
            logEmCSS.Attributes.Add("rel", "stylesheet");
            logEmCSS.Attributes.Add("type", "text/css");        
            logEmCSS.Attributes.Add("href", ExtractBaseLogEmUrl(Context.Request.Url.Segments) + "stylesheet");
            _head.Controls.Add(logEmCSS);

            Page.Controls.Add(_head);

            _head.RenderControl(_writer);
            _writer.WriteLine();
        }

        /// <summary>
        /// Writes the HTML Body element.
        /// </summary>
        protected virtual void RenderHtmlBody()
        {
            // Create the body tag and set it up so that Render calls
            // will have it available
            _body = new HtmlGenericControl("body");

            if (_showHeader)
                RenderPageHeader();

            RenderContents();

            if (_showFooter)
                RenderPageFooter();

            Page.Controls.Add(_body);
            _body.RenderControl(_writer);
            _writer.WriteLine();
        }

        /// <summary>
        /// Creates the top section of every page (navigation, banner, etc).
        /// </summary>
        protected virtual void RenderPageHeader()
        {
            HtmlGenericControl header = new HtmlGenericControl("div");
            header.ID = "header";

            _body.Controls.Add(header);
        }

        /// <summary>
        /// Creates the footer section at the bottom of the page.
        /// </summary>
        protected virtual void RenderPageFooter()
        {
            HtmlGenericControl footer = new HtmlGenericControl("div");
            footer.ID = "footer";

            footer.Controls.Add(BuildPoweredByParagraph());
            footer.Controls.Add(BuildConfigurationInfoParagraph());
            footer.Controls.Add(BuildServerTimestampParagraph());
            footer.Controls.Add(BuildAssemblyInfoParagraph());
            footer.Controls.Add(BuildLogProviderParagraph());

            _body.Controls.Add(footer);
        }
        #endregion

        /// <summary>
        /// This is the only function that should have to be overriden
        /// most of the time: render the contents of a specific page.
        /// </summary>
        protected virtual void RenderContents()
        {
            HtmlGenericControl contents = new HtmlGenericControl("div");
            contents.ID = "contents";

            HtmlGenericControl h1 = new HtmlGenericControl("h1");
            h1.InnerText = "Welcome to LogEm!";
            contents.Controls.Add(h1);

            _body.Controls.Add(contents);
        }

        #region Helpers
        protected HtmlGenericControl BuildConfigurationInfoParagraph()
        {
            HtmlGenericControl buildInfo = new HtmlGenericControl("p");
            buildInfo.InnerText = "Configuration: " + Server.HtmlEncode(BuildInfo.ExtendedConfiguration);

            return buildInfo;
        }

        protected HtmlGenericControl BuildPoweredByParagraph()
        {
            HyperLink logEm = new HyperLink();
            logEm.NavigateUrl = "http://github.com/tresat/LogEm";
            logEm.Text = "logEm";

            HtmlGenericControl poweredBy = new HtmlGenericControl("p");
            poweredBy.Controls.Add(new LiteralControl("Powered by: "));
            poweredBy.Controls.Add(logEm);

            return poweredBy;
        }

        protected HtmlGenericControl BuildServerTimestampParagraph()
        {
            DateTime now = DateTime.Now;
            StringBuilder sb = new StringBuilder();

            sb.Append("Server date time is: ")
                .Append(now.ToLongDateString())
                .Append(" ")
                .Append(now.ToLongTimeString())
                .Append(" displayed relative to the: ")
                .Append((TimeZone.CurrentTimeZone.IsDaylightSavingTime(now) ? TimeZone.CurrentTimeZone.DaylightName : TimeZone.CurrentTimeZone.StandardName))
                .Append(" zone.");

            HtmlGenericControl timestamp = new HtmlGenericControl("p");
            timestamp.Controls.Add(new LiteralControl(sb.ToString()));

            return timestamp;
        }

        protected HtmlGenericControl BuildAssemblyInfoParagraph()
        {
            DateTime buildDate = BuildInfo.BuildDate();
            DateTime linkDate = BuildInfo.LinkerDate();
            StringBuilder sb = new StringBuilder();

            sb.Append("logEm Build Version: ")
                .Append(Assembly.GetExecutingAssembly().GetName().Version)
                .Append(" built on: ")
                .Append(buildDate.ToLongDateString())
                .Append(" ")
                .Append(buildDate.ToLongTimeString())
                .Append(" linked on: ")
                .Append(linkDate.ToLongDateString())
                .Append(" ")
                .Append(linkDate.ToLongTimeString());

            HtmlGenericControl assemblyInfo = new HtmlGenericControl("p");
            assemblyInfo.Controls.Add(new LiteralControl(sb.ToString()));

            return assemblyInfo;
        }

        protected HtmlGenericControl BuildLogProviderParagraph()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("This log is provided by the ")
                .Append(this.RequestLog.Name)
                .Append(" provider.");

            HtmlGenericControl logProvider = new HtmlGenericControl("p");
            logProvider.Controls.Add(new LiteralControl(sb.ToString()));

            return logProvider;
        }

        protected string ExtractBaseLogEmUrl(String[] pRequestUrlSegments)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < pRequestUrlSegments.Length - 1; i++)
            {
                sb.Append(pRequestUrlSegments[i]);
            }

            return sb.ToString();
        }
        #endregion
    }
}