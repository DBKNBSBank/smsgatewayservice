using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JamaaTech.Smpp.Net.Client;
using SMSGatewayService.Data;

namespace SMSGatewayService.IoC
{
    public class MockSMSSending : ISMSSending
    {
        public void SendSMS(ConcurrentDictionary<string, ISMPP> connections, outSMS s)
        {
            try
            {
                string path = "C:\\SMSTextFiles";
                EventLog.WriteEntry("SMSGatewayService", $"Path : {path}\\{s.destPhoneNo}.txt", EventLogEntryType.Information);
                path = $"{path}\\{s.destPhoneNo}.txt";
                File.AppendAllText(path, $"{s.MessageID} - {s.destPhoneNo} -  {s.MessageContent} \n");

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("SMSGatewayService", $"Error : {ex.Message}  {ex.StackTrace}", EventLogEntryType.Error);
            }
        }

        public async Task SendSMSAsync(ConcurrentDictionary<string, ISMPP> connections, outSMS s)
        {
            await Task.Run(() =>
            {
                SendSMS(connections,s);
            });
        }
    }
}
