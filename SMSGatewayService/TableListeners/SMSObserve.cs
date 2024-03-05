using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSGatewayService.TableListeners
{
    public class SMSObserve
    {
        [Key]
        public decimal MessageID { get; set; }
        public string MessageContent { get; set; }
        public string destPhoneNo { get; set; }
        public System.DateTime submissionDate { get; set; }
        public Nullable<System.DateTime> expiryDate { get; set; }
        public short priorityFlag { get; set; }
        public string status { get; set; }
        public Nullable<System.DateTime> dateSent { get; set; }
        public Nullable<decimal> attemptNo { get; set; }
        public Nullable<System.DateTime> lastSendAttempt { get; set; }
        public short Processing { get; set; }
    }
}
