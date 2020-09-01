using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices.SMS
{
    public class PayamSMSV2 : SMSServiceBase, ISMSService
    {
        public PayamSMSV2(ILogger<PayamSMSV2> logger)
        {
            this.logger = logger;
        }

        const string OrganizationName = "kaspian556";
        const string UserName = "kaspian556";
        const string Password = "123456987";
        const string SenderNumber = "982000446";//"982000446000";
        private readonly ILogger<PayamSMSV2> logger;

        public override async Task<bool> Send(string number, string message)
        {
            PayamSMSV2Service.SMSAPIPortTypeClient cl = new PayamSMSV2Service.SMSAPIPortTypeClient();
            var result = await cl.SendAsync(OrganizationName, UserName, Password, SenderNumber, message, new string[] { number });
            logger.LogDebug($"SMS Sent to {number}. response: {result[0].ID}");
            if (long.TryParse(result[0].ID, out _))
                return true;
            throw new Exception("Error Sending SMS. Code: " + result[0].ID);
        }

        //readonly Dictionary<string, string> ErrorCodes=new Dictionary<string, string>
        //{
        //    {"E223","شماره اختصاصی وارد شده نامعتبر است" },

        //}
    }
}
