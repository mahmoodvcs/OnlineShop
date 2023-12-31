﻿using MahtaKala.Entities;
using MahtaKala.GeneralServices.Payment.PardakhtNovin;
using MahtaKala.SharedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PardakhtNovinWebService;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices.Payment
{
    public class PardakhtNovinService : IBankPaymentService
    {
        private readonly IPathService pathService;
        private readonly DataContext dataContext;
        private readonly ILogger<IBankPaymentService> logger;

        const string merchant_id = "011048201";
        const string username = merchant_id;
        const string password = "440196094";
        const string terminalId = "11054155";
        const string subject = "PardakhtNovin";
        const string certificatePassword = "PardakhtNovin@404306";

        const string shareUsername = "mahtaws";
        const string sharePassword = "Ab123456";

        //public class Options
        //{
        //    public string MerchantId { get; set; }
        //}
        public PardakhtNovinService(IPathService pathService, DataContext dataContext, ILogger<IBankPaymentService> logger)
        {
            this.pathService = pathService;
            this.dataContext = dataContext;
            this.logger = logger;
        }
        public string GetPayUrl(Entities.Payment payment)
        {
            return "https://pna.shaparak.ir/_ipgw_/payment/";
        }

        public async Task<string> GetToken(Entities.Payment payment, string returnUrl)
        {
            string res = await ipg((int)payment.Amount, payment.Id.ToString(), returnUrl);// (int.Parse(amount.Text), resno.Text);
            //NameValueCollection data = new NameValueCollection();
            //data.Add("token", res);
            //data.Add("language", "fa");
            //RedirectWithData(data, );
            return res;
        }
        public static bool validateservercertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors SslPolicyErrors)
        {
            return true;
        }

        public async Task<string> GetReport()
        {
            TechnoIPGWSClient cl = GetClient();
            var list = await cl.getTransactionReportAsync(new TransReportParam
            {
                terminal = terminalId,
                WSContext = GetWSContext(),
                length = 100,
                offset = 0,
                secureMerchantId = merchant_id
            });

            return JsonSerializer.Serialize(list);
        }

        public async Task<string> ipg(int amount, string resnt, string returnUrl)
        {
            try
            {
                TechnoIPGWSClient ipgw = GetClient();
                //ipgw.AllowAutoRedirect = false;
                //  step1 data to sign
                RequestParam requestparam = new RequestParam();
                requestparam.Amount = amount;                      //مبلغ
                requestparam.AmountSpecified = true;
                requestparam.MerchantId = merchant_id;              //کد پذیرنده
                requestparam.TerminalId = terminalId;
                requestparam.TransType = enTransType.enGoods;        //نوع تراکنش خرید
                requestparam.TransTypeSpecified = true;
                requestparam.ReserveNum = resnt;             //شماره فاکتور
                requestparam.RedirectUrl = returnUrl; //"http://192.168.0.53:1007/conf.aspx";// "http://178.252.189.82:1006/conf.aspx";// "http://93.115.150.21:8066/ipgconf.aspx";// 
                requestparam.WSContext = GetWSContext();
                //GenerateTransactionDataToSignResult generateTransactionDataToSignResult = new GenerateTransactionDataToSignResult();
                var generateTransactionDataToSignResult = await ipgw.GenerateTransactionDataToSignAsync(requestparam);
                if (generateTransactionDataToSignResult.@return.Result.ToLower() != "ersucceed")
                {
                    throw new Exception("Error signing data: " + generateTransactionDataToSignResult.@return.Result);
                }
                string datatosign = generateTransactionDataToSignResult.@return.DataToSign;
                string uniqid = generateTransactionDataToSignResult.@return.UniqueId;
                //    step2 sign to token
                //ipgw.ClientCertificates.Clear();
                //ipgw.ClientCertificates.Add(cert);
                //ipgw.AllowAutoRedirect = false;
                GenerateSignedDataTokenParam tokenParams = new GenerateSignedDataTokenParam();
                tokenParams.Signature = datatosign;
                tokenParams.UniqueId = uniqid;
                tokenParams.WSContext = GetWSContext();
                var tokenResult = await ipgw.GenerateSignedDataTokenAsync(tokenParams);
                if (tokenResult.@return.Result.ToLower() != "ersucceed")
                {
                    throw new Exception("Error recieving token: " + tokenResult.@return.Result);
                }
                return tokenResult.@return.Token;

            }
            catch (Exception ex) { return ex.Message; }
        }

        private static WSContext GetWSContext()
        {
            WSContext w = new WSContext();
            w.UserId = username;
            w.Password = password;
            return w;
        }

        private TechnoIPGWSClient GetClient()
        {
            string certpath = Path.Combine(pathService.AppRoot, "PardakhtNovin.cer");
            PardakhtNovinWebService.TechnoIPGWSClient ipgw = new PardakhtNovinWebService.TechnoIPGWSClient();
            //TechnoPaymentWebServiceService ipgw = new TechnoPaymentWebServiceService();
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(validateservercertificate);
            X509Certificate2 cert = new X509Certificate2(certpath, certificatePassword, X509KeyStorageFlags.DefaultKeySet);
            ipgw.ClientCredentials.ClientCertificate.Certificate = cert;
            return ipgw;
        }

        public async Task<Entities.Payment> Paid(string bankReturnBody)
        {
            //TODO: check existence of values
            var dic = bankReturnBody.Split('&').ToDictionary(a => a.Split('=')[0].ToLower(), a => a.Split('=')[1]);
            var token = dic["token"];
            long pid = long.Parse(dic["resnum"]);
            dic.TryGetValue("traceno", out var traceNo);
            dic.TryGetValue("refnum", out var refNum);
            dic.TryGetValue("customerrefnum", out var customerRefNum);

            var payment = dataContext.Payments.Include(a => a.Order).FirstOrDefault(a => a.Id == pid);
            if (payment == null)
            {
                logger.LogError($"Invalid Payment.Id {pid}. Does not exist.");
                throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
            }
            if (payment.PayToken != token)
            {
                logger.LogError($"Payment token does not match. Payment.Id: {pid}. Ours: '{payment.PayToken}'. Bank: '{token}'");
                throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
            }

            payment.ReferenceNumber = refNum;
            payment.TrackingNumber = traceNo;
            payment.PSPReferenceNumber = customerRefNum;

            var state = dic["state"];
            if (state.ToLower() == "ok")
            {
                if (payment.State != PaymentState.SentToBank)
                {
                    TechnoIPGWSClient ipgw = new TechnoIPGWSClient();

                    WSContext wsContext = new WSContext();

                    wsContext.UserId = username;
                    wsContext.Password = password;
                    ReverseMerchantTransParam param = new ReverseMerchantTransParam();
                    param.RefNum = refNum;
                    param.Token = token;
                    param.WSContext = wsContext;
                    var result = await ipgw.ReverseMerchantTransAsync(param);

                }
                else
                {
                    payment.State = PaymentState.PaidNotVerified;
                    await dataContext.SaveChangesAsync();
                    TechnoIPGWSClient ipgw = new TechnoIPGWSClient();

                    WSContext wsContext = new WSContext();

                    wsContext.UserId = username;
                    wsContext.Password = password;

                    VerifyMerchantTransParam verifyParam = new VerifyMerchantTransParam();
                    verifyParam.WSContext = wsContext;
                    verifyParam.Token = token;
                    verifyParam.RefNum = refNum;

                    var result = await ipgw.VerifyMerchantTransAsync(verifyParam);
                    if (result.@return.Result == "erSucceed")
                    {
                        payment.State = PaymentState.Succeeded;
                        payment.Order.State = OrderState.Paid;
                        //TODO: Store CustomerRefNum
                    }
                }
            }
            else
            {
                logger.LogError($"Payment not successful. pid: {payment.Id} - State: {state}");
                payment.State = PaymentState.Failed;
                payment.Order.State = OrderState.Canceled;
            }

            await dataContext.SaveChangesAsync();
            return payment;
        }

        const string ShareApiAddress = "http://178.252.189.82:9000/api/SettlementRequest";
        const string inqueiryAddress = "http://178.252.189.82:9000/api/InquiryTransactionSettlement";

        private async Task<string> Post(string address, string body)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            client.DefaultRequestHeaders.Add("username", shareUsername);
            client.DefaultRequestHeaders.Add("password", sharePassword);

            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(address, content);
            var resStr = await response.Content.ReadAsStringAsync();
            return resStr;
        }

        //public async Task<string> Share(Entities.Payment payment)
        //{

        //    var productIds = 
        //}
        //public async Task<string> Share(Entities.Payment payment, List<ProductPaymentParty> items)
        //{
        //    //            var inqres = await Post(inqueiryAddress, @"[
        //    //	{
        //    //		""referenceNumber"":""026021451836""

        //    //    }
        //    //]");
        //    //var rrn = "26021451836";
        //    SettlementRequest request = new SettlementRequest()
        //    {
        //        referenceNumber = new string('0', 12 - payment.PSPReferenceNumber.Length) + payment.PSPReferenceNumber,
        //        scatteredSettlement = items.Select(a => new ScatteredSettlementDetails
        //        {
        //            settlementIban = a.PaymentParty.ShabaId,
        //            shareAmount 
        //        }).ToList()
        //    };
        //    //foreach (var item in items)
        //    //{
        //    //    request.scatteredSettlement.Add(new ScatteredSettlementDetails
        //    //    {
        //    //        settlementIban = item.PaymentParty.ShabaId,
        //    //        sharePercent = (int)item.Percent
        //    //    });
        //    //}

        //    var reqSgtring = JsonSerializer.Serialize(request);
        //    logger.LogInformation(reqSgtring);
        //    var resStr = await Post(ShareApiAddress, "[" + reqSgtring + "]");
        //    logger.LogInformation(resStr);
        //    return resStr;
        //}

        public async Task SharePayment(Entities.Payment payment, List<PaymentShareDataItem> items)
        {
            SettlementRequest request = new SettlementRequest()
            {
                referenceNumber = new string('0', 12 - payment.PSPReferenceNumber.Length) + payment.PSPReferenceNumber,
                scatteredSettlement = items.GroupBy(a => a.ShabaId).Select(a => new ScatteredSettlementDetails
                {
                    settlementIban = a.Key,
                    shareAmount = (int)a.Sum(c => c.Amount)
                }).ToList()
            };

            var date = DateTime.Now;
            var psItems = items.Select(a => new PaymentSettlement
            {
                Amount = (int)a.Amount,
                Date = date,
                Name = a.Name,
                OrderId = payment.OrderId,
                PaymentId = payment.Id,
                ShabaId = a.ShabaId,
                Status = PaymentSettlementStatus.Sent,
                PayFor = a.PayFor,
                ItemId = a.ItemId
            }).ToList();

            await dataContext.PaymentSettlements.AddRangeAsync(psItems);
            await dataContext.SaveChangesAsync();

            var reqSgtring = "[" + JsonSerializer.Serialize(request) + "]";
            logger.LogInformation(reqSgtring);
            var resStr = await Post(ShareApiAddress, reqSgtring);
            logger.LogInformation(resStr);
            var responses = JsonSerializer.Deserialize<SettlementRequestResponse[]>(resStr);

            if (responses.Length != request.scatteredSettlement.Count)
            {
                logger.LogError($"Payment share response count mismatch. PaymentId: {payment.Id} - Request: {reqSgtring} \r\n\r\nResponse: {resStr}");
                throw new Exception($"Payment share response count mismatch. PaymentId: {payment.Id}");
            }

            for (int i = 0; i < responses.Length; i++)
            {
                foreach (var item in psItems.Where(a => a.ShabaId == request.scatteredSettlement[i].settlementIban))
                {
                    psItems[i].Status = responses[i].status == StatusType.Succeed ? PaymentSettlementStatus.Succeeded : PaymentSettlementStatus.Failed;
                    psItems[i].Response = responses[i].message;
                }
            }
            await dataContext.SaveChangesAsync();
        }

        class PaidReturnData
        {
            public string Token { get; set; }
            public string State { get; set; }
            public string ResNum { get; set; }
            public string TraceNo { get; set; }
            public string CustomerRefNum { get; set; }
            public string RefNum { get; set; }
        }

    }
}
