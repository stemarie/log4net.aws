using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Config;

namespace log4net.aws.console
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            log.Info("test");
            Console.ReadKey();
        }
    }
}
