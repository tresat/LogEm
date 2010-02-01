using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogEm.Logging.RequestLogs.Sql2005RequestLog
{
    public partial class Sql2005Session
    {
        public override Guid? ID
        {
            get { return _SessionID; }
            set { _id = value; }
        }
    }
}
