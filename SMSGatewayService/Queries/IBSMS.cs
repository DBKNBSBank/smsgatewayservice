using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSGatewayService.Queries
{
    public class IBSMS
    {
        emailbankingEntities _entity;
        public IBSMS()
        {
            _entity = new emailbankingEntities();
        }
        public tblOutgoingSMS DeleteIBSMS(tblOutgoingSMS tblOutgoingSM)
        {
           return _entity.tblOutgoingSMS.Remove(tblOutgoingSM);
        }
        public List<tblOutgoingSMS> GetIBMessages()
        {
            return _entity.tblOutgoingSMS.OrderByDescending(s => s.OutID).ToList<tblOutgoingSMS>();
        }
    }
}
