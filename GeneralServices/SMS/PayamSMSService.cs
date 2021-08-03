using MahtaKala.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices
{
    public class ParsGreenSMSService : SMSServiceBase
    {

        private readonly string signature;

        public ParsGreenSMSService(IConfiguration configuration)
        {
            signature = configuration.GetSection("AppSettings")["ParsGreenSignature"];
        }

        public override Task<IEnumerable<ReceivedSMS>> GetReceivedSMSes()
        {
            throw new NotImplementedException();
        }

        public override Task ReadReceivedSMSs()
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> Send(string number, string message)
        {
            ParsGreen.SendSMSSoapClient sms = new ParsGreen.SendSMSSoapClient(ParsGreen.SendSMSSoapClient.EndpointConfiguration.SendSMSSoap);
            var refStr = "";
            var retVal = await sms.SendAsync(signature, number, message, refStr);
            //NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            //logger.Warn(() => $"{number}: {message} - Return Value: {retVal} - refStr: {refStr}");
            if (retVal.Body.SendResult != 1)
                throw new Exception($"Send Error. return code: {retVal} - refStr: {refStr}");
            return true;
        }

        //public static async void SendAsync(IEnumerable<string> numbers, IEnumerable<string> messages)
        //{
        //    PARSGREEN.API.SMS.Send.SendSMS sms = new PARSGREEN.API.SMS.Send.SendSMS();
        //    sms.Send("")
        //    //var mobiles = new ir.payamkotah.ArrayOfString();
        //    //foreach (var n in numbers)
        //    //{
        //    //    mobiles.Add(n);
        //    //}
        //    //var msgs = new ir.payamkotah.ArrayOfString();
        //    //foreach (var m in messages)
        //    //{
        //    //    msgs.Add(m);
        //    //}
        //    //ir.payamkotah.SMSPanelSoapClient cl = new ir.payamkotah.SMSPanelSoapClient();
        //    //var response = await cl.SendAsync(AppSettings.SMSUserName, AppSettings.SMSPassword, AppSettings.SMSNumber, mobiles, msgs, false, false, false);
        //    //cl.stat
        //    //response.Body.
        //}
    }

    public class PayamSMSService : SMSServiceBase
    {
        private const string BaseUrl = "http://www.payamsms.com/rest";
        private readonly string apikey;
        private readonly string smsNumber;
        private ILogger<PayamSMSService> logger;
        public PayamSMSService(IConfiguration configuration, ILogger<PayamSMSService> logger)
        {
            apikey = configuration.GetSection("AppSettings")["PayamSMSApiKey"];
            smsNumber = configuration.GetSection("AppSettings")["PayamSMSNumber"];
            this.logger = logger;
        }

        public override Task ReadReceivedSMSs()
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<ReceivedSMS>> GetReceivedSMSes()
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> Send(string number, string message)
        {
            HttpClient _client = new HttpClient();
            var payload = new Dictionary<string, string>
            {
              {"apikey", apikey},
              {"from", smsNumber},
              {"to", number},
              {"content", message}
            };

            string strPayload = JsonConvert.SerializeObject(payload);
            HttpContent c = new StringContent(strPayload, Encoding.UTF8, "application/json");
            string res = "";
            HttpResponseMessage httpResponse = null;
            try
            {
                httpResponse = await _client.PostAsync($"{BaseUrl}/send", c);
                res = await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ServiceMessages.SMS.SendNetworkError, ex);
            }
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception(string.Format(ServiceMessages.SMS.SendHttpError, $"{httpResponse.StatusCode} {res}"));
            }
            var result = JsonConvert.DeserializeObject<SMSResultModel>(res);
            if (result.status == 0)
                return true;

            if (!ServiceMessages.SMS.ServiceErrors.TryGetValue(result.status, out var err))
                err = result.status.ToString();
            throw new Exception(ServiceMessages.SMS.SendError + err);
        }
    }
}
