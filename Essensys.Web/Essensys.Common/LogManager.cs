using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essensys.Common
{
    /// <summary>
    /// Manager de Log4Net
    /// </summary>
    public static class LogManager
    {
        public static void Initialise()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static void LogError(object oMessage, Exception oException)
        {
            log4net.LogManager.GetLogger("Essensys").Fatal(oMessage, oException);
        }
        public static void LogTrace(object oMessage, Exception oException)
        {
            log4net.LogManager.GetLogger("Essensys").Debug(oMessage, oException);
        }
    }
}
