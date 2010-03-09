using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogEm.Logging.RequestLogs.Sql2005RequestLog
{
    public partial class Sql2005Session
    {
        public override Guid? IDBase
        {
            get { return _SessionID; }
        }
        public override string BrowserBase
        {
            get { return _Browser; }
        }
        public override int? MajorVersionBase
        {
            get { return _MajorVersion; }
        }
    }
}
