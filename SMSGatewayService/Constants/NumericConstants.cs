using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSGatewayService.Constants
{
    public class NumericConstants
    {
        public static Int32 ReconnectInteval = 10000;
        public static Int32 AutoReconnectDelay = 5000;
        public static Int32 KeepAliveInterval = 5000;

        public const Int32 CLOSE_CONNECTION = 600;
    }
}
