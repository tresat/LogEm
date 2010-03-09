using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LogEm.Logging
{
    public class SessionBase
    {
        #region "Member Vars"
        protected Guid? _id;
        protected HttpContext _context;
        #endregion

        #region "Properties"
        public virtual Guid? IDBase
        {
            get { return _id; }
        }
        public virtual string BrowserBase
        {
            get { return null; }
        }
        public virtual int? MajorVersionBase
        {
            get { return null; }
        }
        #endregion

        #region Constructors
        public SessionBase()
        {
            _id = Guid.NewGuid();
        }
        #endregion
    }
}
