using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;


namespace Kooboo.TestSupport
{
    public static class ContextTestFixture
    {
        public static void Initialize()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");            
        }
    }
}
