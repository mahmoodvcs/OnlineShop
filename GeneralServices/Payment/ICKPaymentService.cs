using Newtonsoft.Json;
using SevenTiny.Bantina.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices.Payment
{
    public class ICKPaymentService : IBankPaymentService
    {
        const string merchantId = "02000001";
        const string password = "127138AAFF124578";
        const string rsaPublicKey = @"MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCluwJqxVspFIgX5j82vs2g+7BT
xdLWm+M3fKUoII7N/1737OxTdpK957qeZ1d7ZExfgmLDPrx7DOGUsF7p8QYddUMt
lLz0pKE5lSEJkoGu+CxrZzls3PfKoUtzizpTRQiAs+TDorREiK6Zp0oHA8MzxBEM
2d6HIwOT/IAO6UC+CQIDAQAB";

        const string baseUrl = "https://ikc.shaparak.ir/";
        const string retrunUrl = "http://localhost:55741/Payment/Payed";

        public async Task<string> GetToken(long amount)
        {

            var bStr = $"{merchantId}{password}{amount:000000000000}00";

            using (var aes = new AesCryptoServiceProvider())
            {
                var bytes = Encrypt(StringToByteArray(bStr), aes.Key, aes.IV);
                var hashed = SHA256Managed.Create().ComputeHash(bytes);
                var aa = aes.Key.Concat(hashed).ToArray();
                var data = ByteArrayToString(RSACommon.Encrypt(aa, rsaPublicKey));
                var tokenRequest = new TokenReuqest
                {
                    authenticationEnvelope = new AuthenticationEnvelope
                    {
                        data = data,
                        iv = ByteArrayToString(aes.IV)
                    },
                    request = new IKCRequest
                    {
                        amount = amount,
                        paymentId = null,
                        transactionType = "Purchase",
                        requestTimestamp = (DateTime.Now - new DateTime(1970, 1, 1)).Ticks,
                        revertUri = retrunUrl,
                        requestId = "1",
                        terminalId = "02010523",
                        acceptorId = "111111111111111"
                    }
                };

                HttpClient _client = new HttpClient();
                string strPayload = JsonConvert.SerializeObject(tokenRequest);
                HttpContent c = new StringContent(strPayload, Encoding.UTF8, "application/json");
                string res = "";
                HttpResponseMessage httpResponse = null;
                try
                {
                    httpResponse = await _client.PostAsync($"{baseUrl}api/v3/tokenization/make", c);
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
                var response = JsonConvert.DeserializeObject<TokenResponse>(res);
                return response.result.token;
            }
        }


        public byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.Zeros;

                aes.Key = key;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, encryptor);
                }
            }
        }

        public byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.Zeros;

                aes.Key = key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, decryptor);
                }
            }
        }

        private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                return ms.ToArray();
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public Task<string> GetToken(Entities.Payment payment)
        {
            throw new NotImplementedException();
        }

        public string GetPayUrl(Entities.Payment payment)
        {
            return "";
        }

        class TokenReuqest
        {
            public AuthenticationEnvelope authenticationEnvelope { get; set; }
            public IKCRequest request { get; set; }
        }
        class AuthenticationEnvelope
        {
            public string data { get; set; }
            public string iv { get; set; }
        }
        class IKCRequest
        {
            public string acceptorId { get; set; }
            public long amount { get; set; }
            public string billInfo { get; set; }
            public string cmsPreservationId { get; set; }
            //"multiplexParameters": [
            //{
            //"amount": 550,
            //"iban": "IR870180000000008322908440"
            //},
            //{
            //"amount": 450,
            //"iban": "IR680120010000003187611452"
            //}
            //],
            public string paymentId { get; set; }
            public string requestId { get; set; }
            public long requestTimestamp { get; set; }
            public string revertUri { get; set; }
            public string terminalId { get; set; }
            public string transactionType { get; set; }
        }

        class TokenResponse
        {
            public string description { get; set; }
            public string responseCode { get; set; }
            public TokenResponseResult result { get; set; }
            public bool status { get; set; }
        }

        class TokenResponseResult
        {
            public long expiryTimestamp { get; set; }
            public long initiateTimestamp { get; set; }
            public string token { get; set; }
            public string transactionType { get; set; }
        }


    }

}
