using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SMSGatewayService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                EventLog.CreateEventSource("SMSGatewayService", "SMSGatewayService");
                
                EventLog.WriteEntry("SMSGatewayService", $"{eventArgs.Exception.ToString()}", EventLogEntryType.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new SMSGatewayService()
            };
            ServiceBase.Run(ServicesToRun);
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            EventLog.WriteEntry("SMSGatewayService", $"{e.ExceptionObject.ToString()}", EventLogEntryType.Error);
        }
    }
}
