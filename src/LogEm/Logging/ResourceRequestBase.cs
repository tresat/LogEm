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
        #endregion

        #region "Properties"
        public virtual Guid? BaseID
        {
            get { return _id; }
            set { _id = value; }
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
