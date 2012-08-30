using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ZenDeskApi.Model;
using log4net.Core;
using ZenDesk = ZenDeskApi.ZenDeskApi;

namespace log4net.Appender
{
    public class ZendeskAppender : BufferingAppenderSkeleton
    {
        private string _url;
        private string _user;
        private string _password;
        private int _requesterId;
        private string _tags;

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
                    StringWriter writer = new StringWriter();
                    Layout.Format(writer,loggingEvent);
                    
                    Ticket ticket = new Ticket
                        {
                            Description = writer.ToString(),
                            Subject = string.Format("{0} {1} {2} {3}",
                            loggingEvent.TimeStamp,
                            loggingEvent.ThreadName,
                            loggingEvent.Level,
                            loggingEvent.LoggerName),
                            SetTags = Tags,
                            RequesterId = RequesterId
                        };
                    client.CreateTicket(ticket);
                });
        }

        public string Url
        {
            get
            {
                if (string.IsNullOrEmpty(_url))
                    throw new ArgumentNullException(@"Url property not defined in appender/config: <Url value='https://zendeskurl.zendesk.com'>");
                return _url;
            }
            set { _url = value; }
        }

        public string User
        {
            get
            {
                if (string.IsNullOrEmpty(_user))
                    throw new ArgumentNullException(@"User property not defined in appender/config: <User value='zendeskusername/token' />");
                return _user;
            }
            set { _user = value; }
        }

        public string Password
        {
            get
            {
                if (string.IsNullOrEmpty(_password))
                    throw new ArgumentNullException(@"Password property not defined in appender/config: <Password value='zendeskapitoken' />");
                return _password;
            }
            set { _password = value; }
        }

        protected string Tags
        {
            get
            {
                if (string.IsNullOrEmpty(_tags))
                    throw new ArgumentNullException(@"Tags property not defined in appender/config: <Tags value='log4net tags to set' />");
                return _tags;
            }
            set { _tags = value; }
        }

        protected int RequesterId
        {
            get
            {
                if (_requesterId==0)
                    throw new ArgumentNullException(@"RequesterId property not defined in appender/config: <RequesterId value='RequesterId(#)' /> Example: <RequesterId value='1' />");
                return _requesterId;
            }
            set { _requesterId = value; }
        }
    }
}
