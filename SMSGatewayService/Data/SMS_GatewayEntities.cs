using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSGatewayService.Data
{
    public partial class SMS_GatewayEntities : DbContext
    {


        public SMS_GatewayEntities(string connectionString) : base(connectionString)

        {
            Database.SetInitializer<SMS_GatewayEntities>(null);

        }

       

       
        public virtual DbSet<outSMS> outSMS { get; set; }
        public virtual DbSet<SMSC> SMSCs { get; set; }
    }
}
