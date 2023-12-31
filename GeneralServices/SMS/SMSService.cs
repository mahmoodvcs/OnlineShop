﻿using MahtaKala.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices
{
    public interface ISMSService
    {
        Task<bool> Send(string number, string message);
        Task<int> SendOTP(string number, string message);
        Task ReadReceivedSMSs();
        Task<IEnumerable<ReceivedSMS>> GetReceivedSMSes();

    }

    public abstract class SMSServiceBase : ISMSService
    {
        public abstract Task ReadReceivedSMSs();
        public abstract Task<IEnumerable<ReceivedSMS>> GetReceivedSMSes();

        public abstract Task<bool> Send(string number, string message);

        public async Task<int> SendOTP(string number, string message)
        {
            var code = new Random((int)(DateTime.Now.Ticks % int.MaxValue)).Next(10000, 99999);
            var ok = await Send(number, string.Format(message, code));
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
        public int status { get; set; }
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
