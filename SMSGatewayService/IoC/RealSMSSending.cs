using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JamaaTech.Smpp.Net.Client;
using SMSGatewayService.Data;
using SMSGatewayService.Queries;
using SMSGatewayService.Utilities;

namespace SMSGatewayService.IoC
{
    class RealSMSSending : ISMSSending
    {
        public void SendSMS(ConcurrentDictionary<string, ISMPP> connections, outSMS s)
        {
            SMS _smsModel = new SMS();
            var sendNoClient = Utils.ChooseNetwork(connections, s.destPhoneNo);

            EventLog.WriteEntry("SMSGatewayService", $" After choosing which network to send to, { s.destPhoneNo }", EventLogEntryType.Information);

            if (sendNoClient == null)
            {
                _smsModel.UpdateSMSSent(s.MessageID, "Fail");
            }
            else
            {
                var client = sendNoClient.Item2;


                {
                    if (client.CheckConnectionStatus())
                    {

                        s.destPhoneNo = Regex.Replace(s.destPhoneNo, @"(0|265|\+265|\+2650|2650)(\d{9,9})", "265$2");

                        EventLog.WriteEntry("SMSGatewayService", $" Valid TNM Number, Sending message to this TNM number, { s.destPhoneNo }", EventLogEntryType.Information);
                        var sent = client.SendSMS(s, sendNoClient.Item1,() =>
                        {
                            int res = _smsModel.UpdateSMSSent(s.MessageID, "Sent");
                            while (res < 1){
                                res = _smsModel.UpdateSMSSent(s.MessageID, "Sent");
                            }
                            var smscid = connections.FirstOrDefault(c => c.Value == client);
                            _smsModel.UpdateSMSCSMSCount(int.Parse(smscid.Key), true);
                            return true;
                        });
                        if (!sent)
                        {
                            _smsModel.UpdateSMSProcessing(s.MessageID,0);
                        }
                       
                    }
                    else
                    {
                        _smsModel.UpdateSMSProcessing(s.MessageID, 0);
                        EventLog.WriteEntry("SMSGatewayService", $"{s.MessageContent} to {s.destPhoneNo} failed. No connection", EventLogEntryType.Warning);
                    }

                }
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
