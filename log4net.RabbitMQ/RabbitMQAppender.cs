using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using RabbitMQ.Client;
using log4net.Core;

namespace log4net.RabbitMQ
{
    public class RabbitMQAppender : Appender.BufferingAppenderSkeleton
    {
        private readonly IConnection _connection;

        private string _hostName;

        public string HostName
        {
            get
            {
                if (String.IsNullOrEmpty(_hostName))
                    throw new ApplicationException("Host name/path not specified");
                return _hostName;
            }
            set { _hostName = value; }
        }

        private string _userName;

        public string UserName
        {
            get
            {
                if (String.IsNullOrEmpty(_userName))
                    _userName = string.Empty;
                return _userName;
            }
            set { _userName = value; }
        }

        private string _passWord;

        public string Password
        {
            get
            {
                if (String.IsNullOrEmpty(_passWord))
                    _passWord = string.Empty;
                return _passWord;
            }
            set { _passWord = value; }
        }

        private string _virtualHost;

        public string VirtualHost
        {
            get
            {
                if (String.IsNullOrEmpty(_virtualHost))
                    _virtualHost = string.Empty;
                return _virtualHost;
            }
            set { _virtualHost = value; }
        }

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

        private string _exchangeName;

        public string ExchangeName
        {
            get
            {
                if (String.IsNullOrEmpty(_exchangeName))
                    _exchangeName = string.Empty;
                return _exchangeName;
            }
            set { _exchangeName = value; }
        }

        public RabbitMQAppender()
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _passWord,
                VirtualHost = _virtualHost
            };

            _connection = factory.CreateConnection();
        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            using (IModel channel = _connection.CreateModel())
            {
                channel.QueueDeclare(QueueName, true, false, false, null);

                BinaryFormatter bf = new BinaryFormatter();

                foreach (var loggingEvent in events)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bf.Serialize(ms, loggingEvent);

                        byte[] message = ms.ToArray();

                        channel.BasicPublish(_exchangeName, _queueName, null, message);
                    }
                }
            }
        }
    }
}
