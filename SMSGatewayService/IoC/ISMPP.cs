using SMSGatewayService.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSGatewayService.IoC
{
   
    public interface ISMPP
    {
       

        bool CheckConnectionStatus();

        bool SendSMS(outSMS s,string sourceAddress, Func<bool> callback);

        Task<bool> SendSMSAsync(outSMS s,string sourceAddress,Func<bool> callback);

        bool Connect();

        bool Disconnect();

        Task<bool> ConnectAsync();

        Task<bool> DisconnectAsync();


    }
}
