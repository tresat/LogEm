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
        public HttpContext Context
        {
            get { return _context; }
        }

        public virtual Guid? ID
        {
            get { return _id; }
            set { _id = value; }
        }
        #endregion

        #region "Constructors"
        public SessionBase() : this(null) { }

        public SessionBase(HttpContext context)
        {
            // context allowed to be null

            _id = Guid.NewGuid();
            _context = context;
        }
        #endregion
    }
}
