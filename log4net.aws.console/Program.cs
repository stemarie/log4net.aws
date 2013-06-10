using System;

namespace log4net.aws.console
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            //XmlConfigurator.Configure();
            Log.Info("test");
            try
            {
                throw new Exception("throw a message!");
            }
            catch (Exception ex)
            {
                Log.Error("Test exception", ex);
            }
            Console.ReadKey();
        }
    }
}
