using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using LogEm.Utilities;
using LogEm.Logging.RequestLogs;

namespace LogEm.Site.Pages
{
    public class LogEmPageBase : Page
    {
#region Members
        protected RequestLog _log;
        protected HtmlTextWriter _writer;
        protected String _title = "LogEm";
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
        public LogEmPageBase() : base() { }
        #endregion

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
            _writer.WriteLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN" + @"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd");
 
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
            HtmlHead head = new HtmlHead();

            HtmlTitle title = new HtmlTitle();
            title.Text = _title;
            head.Controls.Add(title);

            HtmlLink logEmCSS = new HtmlLink();
            logEmCSS.Attributes.Add("rel", "stylesheet");
            logEmCSS.Attributes.Add("type", "text/css");        
            logEmCSS.Attributes.Add("href", Context.Request.RawUrl + "/stylesheet");
            head.Controls.Add(logEmCSS);

            Page.Controls.Add(head);

            head.RenderControl(_writer);
            _writer.WriteLine();
        }

        /// <summary>
        /// Writes the HTML Body element.
        /// </summary>
        protected virtual void RenderHtmlBody()
        {
            _writer.RenderBeginTag(HtmlTextWriterTag.Body);

            RenderContents();

            _writer.RenderEndTag(); // BODY
            _writer.WriteLine();
        }

        /// <summary>
        /// This is the only function that should have to be overriden
        /// most of the time: render the contents of a specific page.
        /// </summary>
        protected virtual void RenderContents()
        {
            _writer.WriteLine("Welcome to LogEm!");
        }
    }
}