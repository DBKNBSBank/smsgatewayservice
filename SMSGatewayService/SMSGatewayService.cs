using Autofac;
using Autofac.Core;
using JamaaTech.Smpp.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using SMSGatewayService.Constants;
using SMSGatewayService.Data;
using SMSGatewayService.IoC;
using SMSGatewayService.Queries;
using SMSGatewayService.SMPPClient;
using SMSGatewayService.TableListeners;
using SMSGatewayService.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;

using System.Threading.Tasks;
using System.Timers;


namespace SMSGatewayService
{
    public partial class SMSGatewayService : ServiceBase
    {
        public static ConcurrentDictionary<string, ISMPP> connections = new ConcurrentDictionary<string, ISMPP>();
        private static Timer timer;
        public static bool isBusy = false;
        private Autofac.IContainer container;
        SMS _smsModel;
        ISMSSending _smsSending;
        Stopwatch _watch;
        public SMSGatewayService()
        {
            InitializeComponent();
            var builder = new ContainerBuilder();
            container = builder.GetContainer();
            _smsSending = container.Resolve<ISMSSending>();
            _smsModel = new SMS();
            _watch = new Stopwatch();

            try
            {
               new SMSTableListener(container);
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("SMSGatewayService", $"Error : {e.ToString()}", EventLogEntryType.Warning);
            }
        }

        private async   void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            try
            {
                //do this every second
               
                if (isBusy)
                {
                    EventLog.WriteEntry("SMSGatewayService", $"Busy.....", EventLogEntryType.Error);
                    return;
                }

                var connected = Utils.CheckForConnections(connections);

                if (!connected)
                    return;
                //Utils.GetIBMessagesToInsertIntoOutMessages();

                isBusy = true;

                List<outSMS> SMSs = new List<outSMS>();
                try
                {
                    SMSs = _smsModel.GetSMSs();

                }
                catch (Exception exx)
                {
                    EventLog.WriteEntry("SMSGatewayService", $"Error : {exx.Message}", EventLogEntryType.Error);
                    isBusy = false;
                    return;
                }

               
                int numSMSs = SMSs.Count;
                int count = 0;
                _watch.Start();
               foreach(var s in SMSs)
               {


                   try
                   {

                        // string status = s.status;
                        //&& DBNull.Value.Equals(status)
                        if (s.Processing == 1)
                            continue;
                       int result = _smsModel.UpdateSMSProcessing(s.MessageID, 1);
                   
                       if(result > 0 )
                        {
                             await _smsSending.SendSMSAsync(connections, s);
                        }

                   }
                   catch (Exception ex)
                   {
                       
                       EventLog.WriteEntry("SMSGatewayService", $"Error : {ex.Message}  {ex.StackTrace}", EventLogEntryType.Error);
                      
                   }
                    count++;
                    if (count.Equals(numSMSs))
                    {
                        EventLog.WriteEntry("SMSGatewayService", $"{numSMSs} SMSs run in : {_watch.ElapsedMilliseconds/1000} seconds", EventLogEntryType.Warning);
                        _watch.Reset();
                        isBusy = false;
                    }

               }
                _watch.Reset();
                isBusy = false;


            }
            catch(Exception ex)
            {
                EventLog.WriteEntry("SMSGatewayService", $"Error : {ex.Message}", EventLogEntryType.Error);
                isBusy = false;
            }
           


        }
        protected  override void OnStart(string[] args)
        {


            
           
            using (var context = new SMS_GatewayEntities(Utils.connectionString))
                {

                    var smscs = context.SMSCs.Where(s => s.SMSCID == 1 || s.SMSCID == 2).OrderByDescending(s => s.SMSCID).ToList<SMSC>();

                    EventLog.WriteEntry("SMSGatewayService", $"Found these  : {smscs.Count} ", EventLogEntryType.Error);

                smscs.ForEach(async (s) =>
                    {
                       
                        var _parameters = new List<Parameter>();
                        _parameters.Add(new NamedParameter("connection",s));
                        var client = container.Resolve<ISMPP>(_parameters);                       
                        connections.GetOrAdd(s.SMSCID.ToString(), client);
                        await client.ConnectAsync();

                    });
                }
                      
               

                timer = new Timer();
                timer.Interval = 1000;
                timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                timer.AutoReset = true;
                timer.Enabled = true;
           


        }

        protected override void OnStop()
        {
        }

        protected override void OnCustomCommand(int command)
        {
            switch (command)
            {
                case NumericConstants.CLOSE_CONNECTION:
                    EventLog.WriteEntry("SMSGatewayService", $"Closing connection", EventLogEntryType.Error);
                    break;
                default:
                    break;
            }
        }
    }
}
