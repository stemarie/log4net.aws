using System;
using System.Globalization;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using log4net.Core;

namespace log4net.Appender
{
    public class SimpleDBAppender : BufferingAppenderSkeleton
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
            var client = new AmazonSimpleDBClient();
            var request = new PutAttributesRequest();
            foreach (LoggingEvent loggingEvent in events)
            {
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
                request.DomainName = "karelllogging";
                request.ItemName = Guid.NewGuid().ToString();
            }
            PutAttributesResponse response = client.PutAttributes(request);
        }
    }
}