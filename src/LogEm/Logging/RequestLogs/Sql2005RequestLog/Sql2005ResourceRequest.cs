using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogEm.Logging.RequestLogs.Sql2005RequestLog
{
    public partial class Sql2005ResourceRequest
    {
        public override Guid? ID
        {
            get { return _ResourceRequestID; }
            set { _id = value; } 
        }
    }
}
