using SMSGatewayService.Data;
using SMSGatewayService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSGatewayService.Queries
{
    class SMPPConnection
    {
        SMS_GatewayEntities _entity;
        public SMPPConnection()
        {
            _entity = new SMS_GatewayEntities(Utils.connectionString);
        }
        public List<SMSC> GetClients()
        {
           
            return _entity.SMSCs.Where(s => s.SMSCID == 1 || s.SMSCID == 2).ToList<SMSC>();
        }
    }
}
