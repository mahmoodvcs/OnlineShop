using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.GeneralServices.MessageParser
{
    public interface IDeliveryCodeReceiver
    {
        (bool, string) CheckReceivedCode(ReceivedSMS receivedSMS);
    }
}
