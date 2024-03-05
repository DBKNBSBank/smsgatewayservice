using SMSGatewayService.Constants;
using SMSGatewayService.Data;
using SMSGatewayService.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSGatewayService.Queries
{
    public class SMS
    {
        //SMS_GatewayEntities _entity;
        string _connectionString;
        public SMS()
        {
            _connectionString = Utils.connectionString;
            EventLog.WriteEntry("SMSGatewayService", $"Connection string : {_connectionString}", EventLogEntryType.Information);
        }
        public void UpdateSMSCSMSCount(int SMSCID,bool success)
        {
            using (var _entity = new SMS_GatewayEntities(_connectionString))
            {
                var smsc = _entity.SMSCs.FirstOrDefault(s => s.SMSCID == SMSCID);
                if(smsc != null)
                {
                    if (success)
                        smsc.CountSent = smsc.CountSent + 1;
                    else
                        smsc.CountFailed = smsc.CountFailed + 1;
                    _entity.SaveChanges();
                }
            }
        }
        public int ProcessingCount()
        {
            using (var _entity = new SMS_GatewayEntities(_connectionString))
            {
                string query = "SELECT MessageID FROM outSMS WHERE(status IS NULL) AND Processing = 1  " +
                " AND((expiryDate IS NULL AND(DATEDIFF(hh, submissionDate, { fn NOW() }) < 24)) OR expiryDate > { fn NOW()})";

                return _entity.Database
                        .SqlQuery<outSMS>(query).ToList<outSMS>().Count();
            }

        }
        public List<outSMS> ResetQueue()
        {
            using (var _entity = new SMS_GatewayEntities(_connectionString))
            {
                string query = "UPDATE [SMS_Gateway].[dbo].[outSMS] SET Processing = 0 WHERE Processing = 1 AND (status IS NULL) " +
                    "AND ((expiryDate IS NULL AND(DATEDIFF(hh, submissionDate, { fn NOW() }) < 24)) OR expiryDate > { fn NOW()})";

                return _entity.Database.SqlQuery<outSMS>(query).ToList<outSMS>();
            }
        }
        public List<outSMS> GetSMSs()
        {
            using (var _entity = new SMS_GatewayEntities(_connectionString))
            {
                var messagesPerQuery = int.Parse(REGISTRY.MAINREGISTRY.GetValue("MessagesPerQuery").ToString());
                messagesPerQuery = messagesPerQuery != null ? messagesPerQuery : 100;
                _entity.Database.CommandTimeout = 36000;
                string limitNumbers = "";
                if(REGISTRY.MAINREGISTRY.GetValue("TestMode").ToString().Equals("1"))
                    limitNumbers = TestNumbers();
                string query = $"SELECT TOP {messagesPerQuery.ToString()} * FROM outSMS WHERE (status IS NULL) AND Processing=0  " +
                          $"{limitNumbers} AND((expiryDate IS NULL " +
                          "AND(DATEDIFF(hh, submissionDate, { fn NOW() }) < 24)) " +
                          "OR expiryDate > { fn NOW()}) ORDER BY priorityFlag ASC";
               
                return _entity.Database
                          .SqlQuery<outSMS>(query).ToList<outSMS>();
               
            }
                
       
       }
       private string TestNumbers()
        {
            string query = " ";
            string testNumbers = REGISTRY.MAINREGISTRY.GetValue("TestPhoneNumbers").ToString();
            string[] testNumbersArray = testNumbers.Split(';');
            if(testNumbersArray.Count() > 0)
            {
                query += " AND (";
            }
            foreach(var num in testNumbersArray)
            {

                query += $" destPhoneNo={num}";
                if (!testNumbersArray.LastOrDefault().Equals(num))
                {
                    query += " OR ";
                }
            }
            return query+")";
        }
       public outSMS InsertSMSSent(outSMS sms)
        {
            using (var _entity = new SMS_GatewayEntities(_connectionString))
            {
                return _entity.outSMS.Add(sms);
            }
        }
        public void SeedSMSs(int number)
        {
            string[] numbers = { "91286653", "0881286653", "1286622", "1286623", "088128665" };
            using (var _entity = new SMS_GatewayEntities(_connectionString))
            {
                for(int i = 0; i< number; i++)
                {
                    var sms = new outSMS()
                    {
                        destPhoneNo = numbers[new Random().Next(5)],
                        MessageContent = $"Message at this time {DateTime.Now.ToString()}",
                        submissionDate = DateTime.Now,
                         
                    };

                    _entity.outSMS.Add(sms);
                    _entity.SaveChanges();
                }
            }
        }
       public int UpdateSMSSent(decimal messageID, string status)
        {
            using (var _entity = new SMS_GatewayEntities(_connectionString))
            {
                _entity.Database.CommandTimeout = 36000;
                var sms = _entity.outSMS.Where(s => s.MessageID == messageID).FirstOrDefault();
                if (sms != null)
                {

                    //dateSent = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', status = '{status}'
                    sms.dateSent = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    sms.status = status;
                    //sms.Processing = 0;
                    return _entity.SaveChanges();
                }
                return 0;
            }
        }
        public int UpdateSMSProcessing(decimal messageID, short processing)
        {
            using (var _entity = new SMS_GatewayEntities(_connectionString))
            {
                _entity.Database.CommandTimeout = 36000;
                var sms = _entity.outSMS.Where(s => s.MessageID == messageID).FirstOrDefault();
                if (sms != null)
                {

                    //dateSent = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', status = '{status}'
                    //sms.dateSent = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    sms.Processing = processing;

                    return _entity.SaveChanges();
                }
                return 0;
            }
        }
    }
}
