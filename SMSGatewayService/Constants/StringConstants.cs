using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSGatewayService.Constants
{
    class StringConstants
    {
        //public static string AIRTELREGEX = @"^(0|265|\+265)(99|98)[0-9]{7,7}$";
        //public static string TNMREGEX = @"^(0|265|\+265)(88)[0-9]{7,7}$";
        //public static string PHONEREGEX = @"^(0|265|\+265)(88|99|98)[0-9]{7,7}$";

        //public static string AIRTELREGEX = @"^(0|265|\+265)(99|98)[0-9]{7,7}$";
        //public static string TNMREGEX    = @"^(0|265|\+265)(88|89)[0-9]{7,7}$";
        //public static string PHONEREGEX =  @"^(0|265|\+265)(88|89|99|98)[0-9]{7,7}$";
        public static string AIRTELREGEX = @"^(\+2659|\+2658|09|08)[0-9]{8}$";
        public static string TNMREGEX = @"^(\+2659|\+2658|09|08)[0-9]{8}$";
        public static string PHONEREGEX = @"^(0|265|\+265)(88|89|99|98)[0-9]{7,7}$";

        //Accepting all numbers
        //public static string PHONEREGEX = @"^(0|265|\+265)(80|81|82|83|84|85|86|87|88|88|89|99|98|97|96|95|94|93|92|91|90)[0-9]{7,7}$";
        public static string TNM = "TNM";
        public static string AIRTEL = "AIRTEL";
        public static string OTHERNETWORK = "OTHER";
        public static string GATEWAYREGISTRY = "Software\\SMSGateway";
    }
}
