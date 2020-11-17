using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.GeneralServices.SMS
{
    public static class SMSManager
    {
        public static IReadOnlyList<ISMSProcessor> SMSPorcessors { get; } = new List<ISMSProcessor>();

        public static void RegisterProcessor(string processorTypeFullName)
        { 

        }
        public static void RegisterProcessor(ISMSProcessor processor)
        {
            ((List<ISMSProcessor>)SMSPorcessors).Add(processor);
        }

        public static  void SMSReceived(string sender, string body, DateTime receiveDate)
        {
            foreach (var sp in SMSPorcessors)
            {
                try
                {
                    sp.SMSReceived(sender, body, receiveDate);
                }
                catch(Exception ex)
                {
                    //TODO: log
                }
            }
        }
    }
}
