using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogEm.Logging
{
    public class ResourceRequestBase
    {
        #region "Member Vars"
        protected Guid? _id;
        protected String _handler;
        #endregion

        #region "Properties"
        public virtual Guid? ID
        {
            get { return _id; }
            set { _id = value; }
        }
        public virtual String Handler
        {
            get { return _handler; }
            set { _handler = value; }
        }
        #endregion

        #region "Constructors"
        public ResourceRequestBase()
        {
            _id = Guid.NewGuid();
        }
        #endregion
    }
}
