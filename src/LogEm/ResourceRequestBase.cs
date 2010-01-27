using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LogEm
{
    public class ResourceRequestBase
    {
        protected HttpContext _context;

        public HttpContext Context
        {
            get { return _context; }
        }

        public ResourceRequestBase() { }

        public ResourceRequestBase(HttpContext context)
        {
            _context = context;
        }
    }
}
