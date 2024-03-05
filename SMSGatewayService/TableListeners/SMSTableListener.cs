using Autofac;
using SMSGatewayService.Constants;
using SMSGatewayService.Data;
using SMSGatewayService.IoC;
using SMSGatewayService.Queries;
using SMSGatewayService.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;

namespace SMSGatewayService.TableListeners
{
    class SMSTableListener
    {
        private SqlTableDependency<SMSObserve> _dependency;
        private IContainer _container;
        ISMSSending _sending;
        public SMSTableListener(IContainer container)
        {
            _container = container;

            _sending = _container.Resolve<ISMSSending>();

            var _host = REGISTRY.MAINREGISTRY.GetValue("DatabaseHost").ToString();
            var _user = REGISTRY.MAINREGISTRY.GetValue("DatabaseUser").ToString();
            var _password = REGISTRY.MAINREGISTRY.GetValue("DatabasePassword").ToString();
            var _table = REGISTRY.MAINREGISTRY.GetValue("DatabaseTable").ToString();
            var connectionString = $"data source={_host};initial catalog={_table};integrated security=False;user id={_user};password={_password};";
            var mapper = new ModelToTableMapper<SMSObserve>();
            mapper.AddMapping(model => model.MessageID, "MessageID");
            _dependency = new SqlTableDependency<SMSObserve>(connectionString, "outSMS",null, mapper);
            _dependency.OnChanged += _dependency_OnChanged;
            _dependency.OnError += _dependency_OnError;
            _dependency.Start();
        }

        private void _dependency_OnError(object sender, ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async  void _dependency_OnChanged(object sender, RecordChangedEventArgs<SMSObserve> e)
        {
            try
            {
                if (e.ChangeType != ChangeType.None)
                {
                    switch (e.ChangeType)
                    {
                        case ChangeType.Delete:

                            break;
                        case ChangeType.Insert:
                            EventLog.WriteEntry("SMSGatewayService", $"Messge to {e.Entity.destPhoneNo} through Observer", EventLogEntryType.Information);
                            using (var _entity = new SMS_GatewayEntities(Utils.connectionString))
                            {
                               
                                var id = e.Entity.MessageID;
                                var sms = _entity.outSMS.Where(s => s.MessageID == id).FirstOrDefault();
                                SMS _smsModel = new SMS();

                                //string status = e.Entity.status;
                                int result = _smsModel.UpdateSMSProcessing(sms.MessageID, 1);
                                if(result > 0)
                                    await _sending.SendSMSAsync(SMSGatewayService.connections, sms);
                            }
                            break;
                        case ChangeType.Update:

                            break;
                    }

                }
            }
            catch(Exception ex)
            {
                EventLog.WriteEntry("SMSGatewayService", $"Error: {ex.Message}", EventLogEntryType.Error);
            }
        }
    }
}
