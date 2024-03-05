using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMSGatewayService.Data
{
    [Table("SMSC")]
    public class SMSC
    {
        [Key]
        public decimal SMSCID { get; set; }
        public string Description { get; set; }
        public string Host { get; set; }
        public decimal Port { get; set; }
        public string systemID { get; set; }
        public string password { get; set; }
        public string SystemType { get; set; }
        public decimal SequenceNo { get; set; }
        public short addressNPI { get; set; }
        public short addressTON { get; set; }
        public decimal CountSent { get; set; }
        public decimal CountFailed { get; set; }
        public decimal CountReceived { get; set; }
        public string AddressRange { get; set; }
        public string sendAsPhoneNumber { get; set; }
    }
}