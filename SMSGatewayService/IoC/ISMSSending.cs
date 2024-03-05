using JamaaTech.Smpp.Net.Client;
using SMSGatewayService.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSGatewayService.IoC
{
    public interface ISMSSending
    {
        void SendSMS(ConcurrentDictionary<string, ISMPP> connections, outSMS s);
        Task SendSMSAsync(ConcurrentDictionary<string, ISMPP> connections, outSMS s);
    }
}
