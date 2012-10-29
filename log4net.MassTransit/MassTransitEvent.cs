using log4net.Core;

namespace log4net.MassTransit
{
    public class MassTransitEvent
    {
        public LoggingEvent Event { get; set; }
    }
}