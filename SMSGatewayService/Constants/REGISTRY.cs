using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSGatewayService.Constants
{
    public class REGISTRY
    {
        public static RegistryKey MAINREGISTRY = Registry.LocalMachine.OpenSubKey(StringConstants.GATEWAYREGISTRY);
    }
}
