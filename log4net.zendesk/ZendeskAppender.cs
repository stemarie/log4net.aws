using log4net.Appender.Properties;
using log4net.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Tickets;
using ZenDesk = ZendeskApi_v2.ZendeskApi;

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
                    Layout.Format(writer, loggingEvent);

                    Ticket ticket = new Ticket
                        {
                            Description = writer.ToString(),
                            Subject = string.Format("{0} {1} {2} {3}",
                            loggingEvent.TimeStamp,
                            loggingEvent.ThreadName,
                            loggingEvent.Level,
                            loggingEvent.LoggerName),
                            Tags = Tags.Split(';'),
                            RequesterId = RequesterId
                        };
                    client.Tickets.CreateTicket(ticket);
                });
        }

        public string Url
        {
            get
            {
                if (string.IsNullOrEmpty(_url))
                    throw new ArgumentNullException(Resources.UrlPropertyNotDefined);
                return _url;
            }
            set { _url = value; }
        }

        public string User
        {
            get
            {
                if (string.IsNullOrEmpty(_user))
                    throw new ArgumentNullException(Resources.UserPropertyNotDefined);
                return _user;
            }
            set { _user = value; }
        }

        public string Password
        {
            get
            {
                if (string.IsNullOrEmpty(_password))
                    throw new ArgumentNullException(Resources.PasswordPropertyNotDefined);
                return _password;
            }
            set { _password = value; }
        }

        protected string Tags
        {
            get
            {
                if (string.IsNullOrEmpty(_tags))
                    throw new ArgumentNullException(Resources.TagsPropertyNotDefined);
                return _tags;
            }
            set { _tags = value; }
        }

        protected int RequesterId
        {
            get
            {
                if (_requesterId == 0)
                    throw new ArgumentNullException(Resources.RequesterIdPropertyNotDefined);
                return _requesterId;
            }
            set { _requesterId = value; }
        }
    }
}
