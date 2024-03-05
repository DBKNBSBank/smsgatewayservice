using JamaaTech.Smpp.Net.Client;
using JamaaTech.Smpp.Net.Lib;
using SMSGatewayService.Constants;
using SMSGatewayService.Data;
using SMSGatewayService.IoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSGatewayService.SMPPClient
{
    public class JamaaTechClient : ISMPP
    {
        SmppClient _client;
        SMSC _connection;

       

        public JamaaTechClient(SMSC connection)
        {
            _connection = connection;
            _client = new SmppClient();
            _client.Name = _connection.Description;
            //client.SmppEncodingService = new SmppEncodingService(System.Text.Encoding.UTF8);

            _client.ConnectionStateChanged += new EventHandler<ConnectionStateChangedEventArgs>(client_ConnectionStateChanged);
            _client.StateChanged += new EventHandler<StateChangedEventArgs>(ClientStateChanged);
            // _client.MessageSent += new EventHandler<MessageEventArgs>(ClientMessageSent);
            _client.MessageDelivered += new EventHandler<MessageEventArgs>(Client_MessageDelivered);
            _client.MessageReceived += new EventHandler<MessageEventArgs>(ClientMessageReceived);

            SmppConnectionProperties properties = _client.Properties;
            properties.SystemID = connection.systemID;
            properties.Password = connection.password;
            properties.Port = int.Parse(connection.Port.ToString());
            properties.Host = connection.Host;
            properties.SystemType = connection.SystemType;
            properties.DefaultServiceType = "5750";
            properties.DefaultEncoding = DataCoding.UCS2;
            properties.AddressTon = TypeOfNumber.National;
            properties.AddressNpi = NumberingPlanIndicator.National;
            _client.AutoReconnectDelay = NumericConstants.AutoReconnectDelay;
            _client.KeepAliveInterval = NumericConstants.KeepAliveInterval;
        }
        public bool SendSMS(outSMS s, string sourceAddress, Func<bool> callback)
        {
            var sms = new TextMessage()
            {
                DestinationAddress = s.destPhoneNo,
                Text = s.MessageContent,
                SourceAddress = sourceAddress,
            };
            //callback here                  
            callback();
            var state = _client.BeginSendMessage(sms, (IAsyncResult result) =>
            {
                try
                {
                   

                   _client.EndSendMessage(result);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("SendMessageCompleteCallback:" + ex.ToString());
                }

            }, _client);

            return true;
        }

        public Task<bool> SendSMSAsync(outSMS s,string sourceAddress, Func<bool> callback)
        {
           return Task.Run(() => SendSMS(s,sourceAddress,callback));
        }
        public bool CheckConnectionStatus()
        {
            if (_client.ConnectionState.Equals(SmppConnectionState.Connected))
            {
                return true;
            }
            return false;
        }
        public async Task<bool> ConnectAsync()
        {
            await Task.Run(() => _client.Start());
            return true;
        }

        public async  Task<bool> DisconnectAsync()
        {
            await Task.Run(() => _client.Shutdown());
            return true;
        }
        public bool Connect()
        {
            _client.Start();
            return true;
        }

        public bool Disconnect()
        {
            _client.Shutdown();
            return true;
        }

        private void ClientMessageReceived(object sender, MessageEventArgs e)
        {
            var client = (SmppClient)sender;
            TextMessage msg = e.ShortMessage as TextMessage;
            Console.WriteLine("SMPP client {0} - Message Received from: {1}, msg: {2}", client.Name, msg.SourceAddress, msg.Text);
        }

        private void Client_MessageDelivered(object sender, MessageEventArgs e)
        {
            var client = (SmppClient)sender;
            TextMessage msg = e.ShortMessage as TextMessage;
            Console.WriteLine("SMPP client {0} - Message Delivered from: {1}, msg: {2}", client.Name, msg.SourceAddress, msg.Text);
        }

        private void ClientStateChanged(object sender, StateChangedEventArgs e)
        {
            var client = (SmppClient)sender;
            Console.WriteLine("SMPP client {0}: {1}", client.Name, e.Started ? "STARTED" : "STOPPED");
        }

        private void client_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            var client = (SmppClient)sender;          
            Console.WriteLine("---SMPP client {1} - State {1}", client.Name, e.CurrentState);
            if (client.LastException != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(client.LastException.ToString());
                Console.ResetColor();
            }

            switch (e.CurrentState)
            {
                case SmppConnectionState.Closed:
                    //Connection to the remote server is lost
                    //Do something here
                    {
                        Console.WriteLine("SMPP client {0} - CLOSED", client.Name); 
                        EventLog.WriteEntry("SMSGatewayService", $"{client.Name} has been disconnected- { REGISTRY.MAINREGISTRY.GetValue("TNMConnection").ToString()}", EventLogEntryType.Warning);
                        e.ReconnectInteval = NumericConstants.ReconnectInteval; //Try to reconnect after Interval in seconds
                        break;
                    }
                case SmppConnectionState.Connected:
                    //A successful connection has been established                   
                    EventLog.WriteEntry("SMSGatewayService", $"{client.Name} has been connected ", EventLogEntryType.Warning);
                    Console.WriteLine("SMPP client {0} - CONNECTED", client.Name);
                    break;
                case SmppConnectionState.Connecting:
                    //A connection attemp is still on progress
                    Console.WriteLine("SMPP client {0} - CONNECTING", client.Name);
                    break;
            }
        }

        
    }
}
