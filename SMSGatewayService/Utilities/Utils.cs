using JamaaTech.Smpp.Net.Client;
using SMSGatewayService.Constants;
using SMSGatewayService.Data;
using SMSGatewayService.IoC;
using SMSGatewayService.Queries;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SMSGatewayService.Utilities
{
    public class Utils
    {
        //public enum SendSMSErrorCodes
        //{
        //    UnKnownError = -1,
        //    Successful = 0,
        //    SMSCNotReadyToSend = 88,
        //    NumberNotAuthorised = 99
        //}
        static string _phoneRegex = REGISTRY.MAINREGISTRY.GetValue("PHONEREGEX").ToString();
        static string _host = REGISTRY.MAINREGISTRY.GetValue("DatabaseHost").ToString();
        static string _user = REGISTRY.MAINREGISTRY.GetValue("DatabaseUser").ToString();
        static string _password = REGISTRY.MAINREGISTRY.GetValue("DatabasePassword").ToString();
        static string _table = REGISTRY.MAINREGISTRY.GetValue("DatabaseTable").ToString();
        public static string connectionString = $"Persist Security Info=False;User ID={_user};Password={_password};Initial Catalog={_table};Server={_host}";
       
        static SMS _smsModel = new SMS();
       
        public static void GetIBMessagesToInsertIntoOutMessages()
        {
            using (var context = new emailbankingEntities())
            {
                var SMSs = context.tblOutgoingSMS.OrderByDescending(s => s.OutID).ToList<tblOutgoingSMS>();
                SMSs.ForEach(s =>{
                     var msgBody = s.OutgoingMessage;
                        var regexPIN = new Regex(@"Thank you for Registering for EazyMobile. Your Pin is (\d+)", RegexOptions.IgnoreCase
                                                    | RegexOptions.Multiline
                                                    | RegexOptions.CultureInvariant
                                                    | RegexOptions.Compiled
                                                );
                        string regexReplacePIN = "Your EazyMobile PIN is $1. " + Environment.NewLine.ToCharArray() 
                                                  + "To check your a/c balance reply" + " with 'BAL $1 ALL'." 
                                                  + Environment.NewLine.ToCharArray() + "For more EazyMobile commands sms 'HELP'"
                                                  + " to 322." + Environment.NewLine.ToCharArray() + "NBS Bank.";
                        var regexIBOneOffKey = new Regex(@"Your EazyBanking One Off Key is (\d+). You shall use this to" + " change your IBPassword.", RegexOptions.IgnoreCase
                                                            | RegexOptions.Multiline
                                                            | RegexOptions.CultureInvariant
                                                            | RegexOptions.Compiled
                                                         );
                        string regexReplaceIBOneOffKey = "Your one off key is $1. Use this key to change your Internet" + " Banking Password." 
                                                         + Environment.NewLine.ToCharArray() 
                                                         + "Visit https://eazybanking.nbsmw.com for " + "NBS Bank Internet Banking.";
                    if (regexPIN.IsMatch(regexReplacePIN) || regexIBOneOffKey.IsMatch(regexReplaceIBOneOffKey))
                    {
                        if (regexIBOneOffKey.IsMatch(regexReplaceIBOneOffKey))
                            msgBody = regexIBOneOffKey.Replace(msgBody, regexReplaceIBOneOffKey);
                        if (regexPIN.IsMatch(regexReplacePIN))
                            msgBody = regexPIN.Replace(msgBody, regexReplacePIN);
                        //to insert
                        using (var c = new SMS_GatewayEntities(connectionString))
                        {
                            var mess = new outSMS();
                            mess.destPhoneNo = s.OutPhoneNumber;
                            mess.MessageContent = msgBody;
                            //mess.priorityFlag
                            c.outSMS.Add(mess);
                            c.SaveChanges();
                        }
                        //delete
                        context.tblOutgoingSMS.Remove(s);
                        context.SaveChanges();
                    }
                });
            }
        }
   
        public static bool CheckForConnections(IDictionary<string,ISMPP> connections)
        {
            foreach (KeyValuePair<string, ISMPP> connection in connections)
            {
                return connection.Value.CheckConnectionStatus();
              
            }
            return false;
        }
        public static ISMPP ChooseAnyConnected(IDictionary<string, ISMPP> connections)
        {
            SmppClient client = new SmppClient();
            foreach (KeyValuePair<string, ISMPP> connection in connections)
            {
                if (connection.Value.CheckConnectionStatus())
                {
                    return connection.Value;
                    
                }
            }
            return null;
        }
        public static Tuple<string, ISMPP, string> ChooseNetwork(IDictionary<string, ISMPP> connections,string destNo)
        {
            //var phoneRegex = new Regex(_phoneRegex,
            //                           RegexOptions.Multiline
            //                           | RegexOptions.CultureInvariant
            //                           | RegexOptions.Compiled);
            ISMPP client;

            if (!PhoneValidatePhoneNumber(destNo))
            {
                EventLog.WriteEntry("SMSGatewayService", $" Valid Phone Number, { destNo}", EventLogEntryType.Information);
                return null;
            }
            //var tnmRegex = StringConstants.TNMREGEX;

            //var airtelRegex = StringConstants.AIRTELREGEX;

            //var tnmRegexFromRegistry = new Regex(REGISTRY.MAINREGISTRY.GetValue("TNMREGEX").ToString());

            //var airtelRegexFromRegistry = new Regex(REGISTRY.MAINREGISTRY.GetValue("AIRTELREGEX").ToString());

            string sendingNo;
            string carrier;
            if (TNMValidatePhoneNumber(destNo))
            {
                sendingNo = Properties.Settings.Default.SendFrom;
                client = connections[Properties.Settings.Default.TnmID];                
                carrier = StringConstants.TNM;
                EventLog.WriteEntry("SMSGatewayService", $" Valid TNM Number, Sending message to this TNM number, { destNo}", EventLogEntryType.Information);

            }
            else if (AirtelValidatePhoneNumber(destNo))
            {
                sendingNo = Properties.Settings.Default.SendFrom;
                client = connections[Properties.Settings.Default.AirtelID];
                carrier = StringConstants.AIRTEL;
                EventLog.WriteEntry("SMSGatewayService", $" Valid Airtel Number, Sending message to this Airtel Number, { destNo}", EventLogEntryType.Information);
            }
            else
            {
                client = Utils.ChooseAnyConnected(connections);                
                sendingNo = Properties.Settings.Default.SendFrom;               
                carrier = StringConstants.OTHERNETWORK;
            }

            if (!client.CheckConnectionStatus())
            {
                client = Utils.ChooseAnyConnected(connections);
            }

            return Tuple.Create(sendingNo, client, carrier);

        }
        public static bool OpenConnection(string key)
        {
            var client = SMSGatewayService.connections[key];
            if (client != null)
            {
                client.Connect();
                return true;
            }
            return false;
        }
        static bool PhoneValidatePhoneNumber(string phoneNumber)
        {
            // Define the regular expression pattern
            string pattern = @"^(\+2659|\+2658|2659|2658|09|08)[0-9]{8}$";

            // Create a Regex object and match the pattern against the phone number
            Regex regex = new Regex(pattern);
            Match match = regex.Match(phoneNumber);

            // Return true if the phone number matches the pattern, otherwise false
            return match.Success;
        }
        static bool AirtelValidatePhoneNumber(string phoneNumber)
        {
            // Define the regular expression pattern
            string pattern = @"^(\+2659|2659|09)[0-9]{8}$";

            // Create a Regex object and match the pattern against the phone number
            Regex regex = new Regex(pattern);
            Match match = regex.Match(phoneNumber);

            // Return true if the phone number matches the pattern, otherwise false
            return match.Success;
        }
        static bool TNMValidatePhoneNumber(string phoneNumber)
        {
            // Define the regular expression pattern
            string pattern = @"^(\+2658|2658|08)[0-9]{8}$";

            // Create a Regex object and match the pattern against the phone number
            Regex regex = new Regex(pattern);
            Match match = regex.Match(phoneNumber);

            // Return true if the phone number matches the pattern, otherwise false
            return match.Success;
        }
        public static bool CloseConnection(string key)
        {
            var client = SMSGatewayService.connections[key];
            if(client != null)
            {
                client.Disconnect();
                return true;
            }
            return false;
        }
      
    }
}
