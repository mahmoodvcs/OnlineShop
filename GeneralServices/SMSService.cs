using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices
{
    public interface ISMSService
    {
        Task<bool> Send(string number, string message);
        Task<int> SendOTP(string number, string message);
    }

    //public class ParsGreenSMSService : ISMSService
    //{

    //    private readonly string signature;

    //    public ParsGreenSMSService(IConfiguration configuration)
    //    {
    //        signature = configuration.GetSection("AppSettings")["ParsGreenSignature"];
    //    }

    //    public async Task<bool> Send(string number, string message)
    //    {
    //        ParsGreen.SendSMSSoapClient sms = new ParsGreen.SendSMSSoapClient(ParsGreen.SendSMSSoapClient.EndpointConfiguration.SendSMSSoap);
    //        var refStr = "";
    //        var retVal = await sms.SendAsync(signature, number, message, refStr);
    //        //NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    //        //logger.Warn(() => $"{number}: {message} - Return Value: {retVal} - refStr: {refStr}");
    //        if (retVal.Body.SendResult != 1)
    //            throw new Exception($"Send Error. return code: {retVal} - refStr: {refStr}");
    //        return true;
    //    }

    //    public async Task<int> SendOTP(string number, string message)
    //    {
    //        var code = new Random().Next(1000, 99999);
    //        var ok = await Send(number, message + " " + code);
    //        if (ok)
    //            return code;
    //        return 0;
    //    }
    //    //public static async void SendAsync(IEnumerable<string> numbers, IEnumerable<string> messages)
    //    //{
    //    //    PARSGREEN.API.SMS.Send.SendSMS sms = new PARSGREEN.API.SMS.Send.SendSMS();
    //    //    sms.Send("")
    //    //    //var mobiles = new ir.payamkotah.ArrayOfString();
    //    //    //foreach (var n in numbers)
    //    //    //{
    //    //    //    mobiles.Add(n);
    //    //    //}
    //    //    //var msgs = new ir.payamkotah.ArrayOfString();
    //    //    //foreach (var m in messages)
    //    //    //{
    //    //    //    msgs.Add(m);
    //    //    //}
    //    //    //ir.payamkotah.SMSPanelSoapClient cl = new ir.payamkotah.SMSPanelSoapClient();
    //    //    //var response = await cl.SendAsync(AppSettings.SMSUserName, AppSettings.SMSPassword, AppSettings.SMSNumber, mobiles, msgs, false, false, false);
    //    //    //cl.stat
    //    //    //response.Body.
    //    //        

    //    //}
    //}

    public class PayamSMSService : ISMSService
    {
        private const string BaseUrl = "http://www.payamsms.com/rest";
        private readonly string apikey;
        private readonly string smsNumber;
        public PayamSMSService(IConfiguration configuration)
        {
            apikey = configuration.GetSection("AppSettings")["PayamSMSApiKey"];
            smsNumber = configuration.GetSection("AppSettings")["PayamSMSNumber"];
        }

        public async Task<bool> Send(string number, string message)
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
            var httpResponse = await _client.PostAsync($"{BaseUrl}/send", c);
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception("Cannot update todo task");
            }
            var res = await httpResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SMSResultModel>(res);
            if (result.status == "0")
                return true;
            return false;

        }

        public async Task<int> SendOTP(string number, string message)
        {
            var code = new Random().Next(1000, 99999);
            var ok = await Send(number, message + " " + code);
            if (ok)
                return code;
            return 0;
        }
    }

    public class SMSResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public int Handle { get; set; }
    }

    public class SMSResultModel
    {
        public string status { get; set; }
        public object id { get; set; }
    }

    public enum SMSOTPType
    {
        ActivationCode = 1,
        EnterCode = 2,
        AcceptCode = 3,
        Code = 4
    }
}
