using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.GeneralServices.SMS
{
    public interface ISMSProcessor
    {
        string SMSReceived(string sender, string body, DateTime receiveDate);
    }
}
