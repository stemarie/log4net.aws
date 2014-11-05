using log4net.Core;
using System;
using System.Configuration;
using System.Globalization;
using System.Xml.Linq;

namespace log4net.Appender
{
    internal class Utility
    {
        internal static Amazon.RegionEndpoint GetRegionEndpoint()
        {
            var regionEndpoint = ConfigurationManager.AppSettings["Log4net.Appender.Amazon.RegionEndpoint"];
            return regionEndpoint == null ? Amazon.RegionEndpoint.USWest2 : Amazon.RegionEndpoint.GetBySystemName(regionEndpoint);
        }

        internal static string GetXmlString(LoggingEvent loggingEvent)
        {
            var xmlMessage = new XElement(
                "LogEntry",
                new XElement("UserName", loggingEvent.UserName),
                new XElement("TimeStamp",
                             loggingEvent.TimeStamp.ToString(CultureInfo.InvariantCulture)),
                new XElement("ThreadName", loggingEvent.ThreadName),
                new XElement("LoggerName", loggingEvent.LoggerName),
                new XElement("Level", loggingEvent.Level.ToString()),
                new XElement("Identity", loggingEvent.Identity),
                new XElement("Domain", loggingEvent.Domain),
                new XElement("CreatedOn", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                new XElement("RenderedMessage", loggingEvent.RenderedMessage)
                );

            string exceptionStr = loggingEvent.GetExceptionString();

            if (!string.IsNullOrEmpty(exceptionStr))
            {
                xmlMessage.Add(new XElement("Exception", exceptionStr));
            }

            return xmlMessage.ToString();
        }
    }
}