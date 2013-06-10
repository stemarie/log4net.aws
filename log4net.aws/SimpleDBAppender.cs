using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using log4net.Appender.Language;
using log4net.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace log4net.Appender
{
    public class SimpleDBAppender : BufferingAppenderSkeleton
    {
        private string _dbName;

        public string DBName
        {
            get
            {
                if (String.IsNullOrEmpty(_dbName))
                    throw new ApplicationException(Resource.DBNameNotSpecified);
                return _dbName;
            }
            set
            {
                _dbName = value;
                Client = InitializeSimpleDB();
            }
        }

        protected AmazonSimpleDBClient Client { get; private set; }

        public AmazonSimpleDBClient InitializeSimpleDB()
        {
            AmazonSimpleDBClient client = new AmazonSimpleDBClient();
            ListDomainsRequest request = new ListDomainsRequest();
            var response = client.ListDomains(request);
            bool found = response.ListDomainsResult.DomainName.Any(d => d == DBName);
            if (!found)
            {
                CreateDomainRequest createDomainRequest =
                    new CreateDomainRequest
                        {
                            DomainName = DBName
                        };
                client.CreateDomain(createDomainRequest);
            }
            return client;
        }

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
            Parallel.ForEach(events, e => UploadEvent(e, Client));
        }

        private void UploadEvent(LoggingEvent loggingEvent, AmazonSimpleDB client)
        {
            var request = new PutAttributesRequest();
            request.Attribute.Add(
                new ReplaceableAttribute
                    {
                        Name = "UserName",
                        Replace = true,
                        Value = loggingEvent.UserName
                    });
            request.Attribute.Add(
                new ReplaceableAttribute
                    {
                        Value = loggingEvent.TimeStamp.ToString(CultureInfo.InvariantCulture),
                        Name = "TimeStamp",
                        Replace = true
                    });
            request.Attribute.Add(
                new ReplaceableAttribute
                    {
                        Value = loggingEvent.ThreadName,
                        Name = "ThreadName",
                        Replace = true
                    });
            request.Attribute.Add(
                new ReplaceableAttribute
                    {
                        Value = loggingEvent.RenderedMessage,
                        Name = "Message",
                        Replace = true
                    });
            request.Attribute.Add(
                new ReplaceableAttribute
                    {
                        Value = loggingEvent.LoggerName,
                        Name = "LoggerName",
                        Replace = true
                    });
            request.Attribute.Add(
                new ReplaceableAttribute
                    {
                        Value = loggingEvent.Level.ToString(),
                        Name = "Level",
                        Replace = true
                    });
            request.Attribute.Add(
                new ReplaceableAttribute
                    {
                        Value = loggingEvent.Identity,
                        Name = "Identity",
                        Replace = true
                    });
            request.Attribute.Add(
                new ReplaceableAttribute
                    {
                        Value = loggingEvent.Domain,
                        Name = "Domain",
                        Replace = true
                    });
            request.Attribute.Add(
                new ReplaceableAttribute
                    {
                        Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                        Name = "CreatedOn",
                        Replace = true
                    });
            request.DomainName = _dbName;
            request.ItemName = Guid.NewGuid().ToString();

            client.PutAttributes(request);
        }
    }
}