using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Management;
using System.Diagnostics;

namespace Kooboo.HealthMonitoring
{
    public class Log
    {
        public static void LogException(Exception e)
        {
            var webEvent = new WebRequestErrorEventWrapper(e.Message, null, 100000, e);
            webEvent.Raise();
        }
    }
}