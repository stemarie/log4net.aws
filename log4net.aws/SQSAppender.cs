using Amazon.SQS;
using Amazon.SQS.Model;
using log4net.Appender.Language;
using log4net.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace log4net.Appender
{
    public class SQSAppender : BufferingAppenderSkeleton
    {
        private string _queueName;

        public string QueueName
        {
            get
            {
                if (String.IsNullOrEmpty(_queueName))
                    throw new ApplicationException(Resource.QueueNameNotSpecified);
                return _queueName;
            }
            set
            {
                _queueName = value;
                Client = InitializeQueue();
            }
        }

        public string QueueUrl { get; private set; }

        internal AmazonSQSClient Client { get; private set; }

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
            Parallel.ForEach(events, l =>
                {
                    SendMessageRequest request =
                        new SendMessageRequest()
                        {
                            MessageBody = Utility.GetXmlString(l),
                            QueueUrl = QueueUrl

                        };

                    Client.SendMessage(request);
                });
        }

        private AmazonSQSClient InitializeQueue()
        {
            var client = new AmazonSQSClient(Amazon.RegionEndpoint.USWest2);
            
            ListQueuesRequest listQueuesRequest = new ListQueuesRequest
                                                      {
                                                          QueueNamePrefix = QueueName
                                                      };
            var listQueuesResponse = client.ListQueues(listQueuesRequest);
            bool found = listQueuesResponse.ListQueuesResult.QueueUrls.Any(s => s == QueueName);

            if (found == false)
            {
                var createQueueResponse = client.CreateQueue(new CreateQueueRequest
                                                                 {
                                                                     QueueName = QueueName
                                                                 });
                QueueUrl = createQueueResponse.CreateQueueResult.QueueUrl;
            }
            else
            {
                QueueUrl = client.GetQueueUrl(
                    new GetQueueUrlRequest
                        {
                            QueueName = _queueName
                        }
                    ).GetQueueUrlResult.QueueUrl;
            }
            return client;
        }
    }
}
