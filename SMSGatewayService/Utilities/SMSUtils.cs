using JamaaTech.Smpp.Net.Client;
using SMSGatewayService.Data;
using SMSGatewayService.IoC;
using SMSGatewayService.Queries;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SMSGatewayService.Utilities
{
    public class SMSUtils
    {
        ISMSSending _sender;

        public SMSUtils(ISMSSending sender)
        {
            _sender = sender;
        }
        public void SendSMS(ConcurrentDictionary<string, ISMPP> connections, outSMS s)
        {
            _sender.SendSMS(connections,s);
        }
        public async Task SendSMSAsync(ConcurrentDictionary<string, ISMPP> connections, outSMS s)
        {
            await _sender.SendSMSAsync(connections,s);
        }
    }
}
