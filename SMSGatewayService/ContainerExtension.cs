using Autofac;
using SMSGatewayService.IoC;
using SMSGatewayService.SMPPClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSGatewayService
{
    public static class ContainerExtension
    {
        public static IContainer GetContainer(this ContainerBuilder builder)
        {

            builder.RegisterType<MockSMSSending>().As<ISMSSending>();
            //builder.RegisterType<JamaaTechClient>().As<ISMPP>();

            builder.RegisterType<RealSMSSending>().As<ISMSSending>();            
            builder.RegisterType<EasySMPPClient>().As<ISMPP>();
            var container = builder.Build();
            return container;
        }
    }
}
