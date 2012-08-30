using System.Collections;
using System.Text;
using System.Threading.Tasks;
using ZenDeskApi.Model;
using log4net.Core;
using ZenDesk = ZenDeskApi.ZenDeskApi;

namespace log4net.Appender
{
    public class ZendeskAppender : BufferingAppenderSkeleton
    {
        /// <summary>
        /// Sends the events.
        /// </summary>
        /// <param name="events">The events that need to be send.</param>
        /// <remarks>
        /// <para>
        /// The subclass must override this method to process the buffered events.
        /// </para>
        /// </remarks>
        protected override void SendBuffer(LoggingEvent[] events)
        {
            ZenDesk client = new ZenDesk(Url, User, Password);
            Parallel.ForEach(events, loggingEvent =>
                {
                    StringBuilder sb = new StringBuilder(20 + loggingEvent.Properties.Count);
                    sb.AppendFormat("Message: {0}\n", loggingEvent.RenderedMessage)
                        .AppendFormat("Domain: {0}\n", loggingEvent.Domain)
                        .AppendFormat("Identity: {0}\n", loggingEvent.Identity)
                        .AppendFormat("Level: {0}\n", loggingEvent.Level)
                        .AppendFormat("Logger Name: {0}\n", loggingEvent.LoggerName)
                        .AppendFormat("Thread Name: {0}\n", loggingEvent.ThreadName)
                        .AppendFormat("Time Stamp: {0}\n", loggingEvent.TimeStamp)
                        .AppendFormat("User Name: {0}\n", loggingEvent.UserName);
                    foreach (DictionaryEntry dictionaryEntry in loggingEvent.Properties)
                    {
                        sb.AppendFormat("Property {0}: {1}\n", dictionaryEntry.Key, dictionaryEntry.Value);
                    }
                    if (loggingEvent.ExceptionObject != null)
                    {
                        sb.AppendFormat("Location - ClassName: {0}\n", loggingEvent.LocationInformation.ClassName)
                            .AppendFormat("Location - FileName: {0}\n", loggingEvent.LocationInformation.FileName)
                            .AppendFormat("Location - FullInfo: {0}\n", loggingEvent.LocationInformation.FullInfo)
                            .AppendFormat("Location - LineNumber: {0}\n", loggingEvent.LocationInformation.LineNumber)
                            .AppendFormat("Location - MethodName: {0}\n", loggingEvent.LocationInformation.MethodName);
                    }
                    if (loggingEvent.ExceptionObject != null)
                    {
                        sb.AppendFormat("Exception Object: {0}\n", loggingEvent.ExceptionObject.ToString())
                            .AppendFormat("Exception Message: {0}\n", loggingEvent.ExceptionObject.Message)
                            .AppendFormat("Exception StackTrace: {0}\n", loggingEvent.ExceptionObject.StackTrace)
                            .AppendFormat("Exception InnerException: {0}\n", loggingEvent.ExceptionObject.InnerException);
                    }

                    Ticket ticket = new Ticket
                        {
                            Description = sb.ToString(),
                            Subject = string.Format("{0} {1} {2} {3} - {4}",
                            loggingEvent.TimeStamp,
                            loggingEvent.ThreadName,
                            loggingEvent.Level,
                            loggingEvent.LoggerName,
                            loggingEvent.RenderedMessage),
                            SetTags = Tags,
                            RequesterId = RequesterId
                        };
                    client.CreateTicket(ticket);
                });
        }

        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        protected string Tags { get; set; }
        protected int RequesterId { get; set; }
    }
}
