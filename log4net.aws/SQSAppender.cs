using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using log4net.Core;

namespace log4net.Appender
{
    public class SQSAppender : BufferingAppenderSkeleton
    {
        public string _queueName;

        public string QueueName
        {
            get
            {
                if (String.IsNullOrEmpty(_queueName))
                    throw new ApplicationException("Queue Name not specified; unable to proceed");
                return _queueName;
            }
            set { _queueName = value; }
        }

        //public SQSAppender()
        //{
        //    var client = new Amazon.SQS.AmazonSQSClient();
        //    ListQueuesRequest request = new ListQueuesRequest
        //        {
        //            QueueNamePrefix = QueueName
        //        };
        //    var listQueuesResponse = client.ListQueues(request);
        //    bool found = listQueuesResponse.ListQueuesResult.QueueUrl.Any(s => s == QueueName);
            
        //    if (found == false)
        //    {
        //        var createQueueResponse = client.CreateQueue(new CreateQueueRequest
        //            {
        //                QueueName = QueueName
        //            });
        //        _queueUrl = createQueueResponse.CreateQueueResult.QueueUrl;
        //    }
        //    else
        //    {
        //        _queueUrl = client.GetQueueUrl(
        //            new GetQueueUrlRequest
        //                {
        //                    QueueName = _queueName
        //                }
        //            ).GetQueueUrlResult.QueueUrl;
        //    }
        //}

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
            var client = new Amazon.SQS.AmazonSQSClient();

            string _queueUrl;

            ListQueuesRequest listQueuesRequest = new ListQueuesRequest
                {
                    QueueNamePrefix = QueueName
                };
            var listQueuesResponse = client.ListQueues(listQueuesRequest);
            bool found = listQueuesResponse.ListQueuesResult.QueueUrl.Any(s => s == QueueName);

            if (found == false)
            {
                var createQueueResponse = client.CreateQueue(new CreateQueueRequest
                    {
                        QueueName = QueueName
                    });
                _queueUrl = createQueueResponse.CreateQueueResult.QueueUrl;
            }
            else
            {
                _queueUrl = client.GetQueueUrl(
                    new GetQueueUrlRequest
                        {
                            QueueName = _queueName
                        }
                    ).GetQueueUrlResult.QueueUrl;
            }

            Parallel.ForEach(events, l =>
                {
                    SendMessageRequest request =
                        new SendMessageRequest()
                            .WithMessageBody(Utility.GetXmlString(l))
                            .WithQueueUrl(_queueUrl);

                    client.SendMessage(request);
                });
        }
    }
}
