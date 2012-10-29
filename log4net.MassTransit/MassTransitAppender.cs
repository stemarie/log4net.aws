using System;
using MassTransit;
using log4net.Core;

namespace log4net.MassTransit
{
    public class MassTransitAppender : Appender.BufferingAppenderSkeleton
    {
        private string _queueName;

        public string QueueName
        {
            get
            {
                if (String.IsNullOrEmpty(_queueName))
                    throw new ApplicationException("Queue name/path not specified");
                return _queueName;
            }
            set { _queueName = value; }
        }

        public MassTransitAppender()
        {
            Bus.Initialize(sbc => sbc.ReceiveFrom(QueueName));
        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            foreach (var loggingEvent in events)
            {
                Bus.Instance.Publish(
                    new MassTransitEvent
                        {
                            Event = loggingEvent
                        }
                    );
            }
        }
    }
}
