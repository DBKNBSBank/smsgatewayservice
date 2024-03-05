using EasySMPP;
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
    public class EasySMPPClient : ISMPP
    {
        SmsClient _client;
        Data.SMSC _connection;
       
        public enum LinkState
        {
            Connected,
            Disconnected,
            Connecting
        }
        LinkState _state = LinkState.Disconnected;
        public EasySMPPClient(Data.SMSC connection)
        {
            _connection = connection;
            _client = new SmsClient(_connection.Description, _connection.Host,
                                    int.Parse(_connection.Port.ToString()),
                                    _connection.systemID, _connection.password,
                                    _connection.SystemType, 
                                    byte.Parse(_connection.addressTON.ToString()),
                                    byte.Parse(_connection.addressNPI.ToString()),
                                    _connection.AddressRange,
                                    int.Parse(_connection.SequenceNo.ToString()));
            _client.OnConnectionStateChange += OnConnectionStateChange;
            _client.OnNewSms += OnNewSms;
            _client.OnLog += OnLog;



        }

        private void OnLog(LogEventArgs e)
        {
            EventLog.WriteEntry("SMSGatewayService", $"Error : {e.Message}", EventLogEntryType.Error);
        }

        private void OnNewSms(NewSmsEventArgs e)
        {
            
            throw new NotImplementedException();
        }

        private void OnConnectionStateChange(int state)
        {
            switch (state)
            {
                case 1:
                case 2:
                case 3:
                    {
                        _state = LinkState.Connecting;
                        break;
                    }

                case 4:
                    {
                        _state = LinkState.Connected;
                        break;
                    }

                case 5:
                case 6:
                case 7:
                    {
                        _state = LinkState.Disconnected;
                        break;
                    }
            }
        }

        public bool CheckConnectionStatus()     {
            return _state == LinkState.Connected && _client.CanSend(); 
            
        }

        public bool Connect()
        {
            _client.Connect();
            EventLog.WriteEntry("SMSGatewayService", $"{_connection.Host}--{_connection.Description} connection", EventLogEntryType.Warning);
            return true;
        }

        public async Task<bool> ConnectAsync()
        {
            await Task.Run(() => Connect());
            return true;
        }

        public bool Disconnect()
        {
            _client.Disconnect();
            return true;
        }

        public async Task<bool> DisconnectAsync()
        {
            await Task.Run(() => Disconnect());
            return true;
        }

        public bool SendSMS(outSMS s, string sourceAddress, Func<bool> callback)
        {
            var result =  _client.SendSms(sourceAddress,s.destPhoneNo,s.MessageContent);
            if (result)
            {
                callback();
            }
            return result;
        }

        public Task<bool> SendSMSAsync(outSMS s, string sourceAddress, Func<bool> callback)
        {
            return Task.Run(() => SendSMS(s, sourceAddress,callback));
        }
    }
}
