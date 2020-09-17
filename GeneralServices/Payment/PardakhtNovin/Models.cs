using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.GeneralServices.Payment.PardakhtNovin
{
    public class SettlementRequest
    {
        public string referenceNumber { get; set; }
        //public string settlementIban { get; set; }
        public List<ScatteredSettlementDetails> scatteredSettlement { get; set; }
    }
    public class ScatteredSettlementDetails
    {
        public string settlementIban { get; set; }
        public int sharePercent { get; set; }
    }
    public class SettlementRequestResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public StatusType status { get; set; }
    }
    public enum StatusType : int
    {
        Succeed = 1,
        Invalid = 2,
        Exception = -1
    }
    public class InquiryTransactionSettlementRequest
    {
        public string referenceNumber { get; set; }
    }
    public class InquiryTransactionSettlementResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public StatusType status { get; set; }
        public TransactionStatus transactionStatus { get; set; }
        public TransactionDetails transactionDetails { get; set; }
    }
    public enum TransactionStatus : int
    {
        None = -1,
        NotFound = 0,
        Settlemented = 1,
        WaitingForSettlement = 2,
        WaitingForDeterminingStatus = 3
    }
    public class TransactionDetails
    {
        public string channelName { get; set; }
        public string acceptorCode { get; set; }
        public string terminalCode { get; set; }
        public string referenceNumber { get; set; }
        public string trackingNumber { get; set; }
        public System.DateTime transactionDate { get; set; }
        public System.DateTime sendToSettlementDate { get; set; }
        public System.DateTime SendToShaparakDate { get; set; }
        public System.DateTime settlementDate { get; set; }
        public long transactionAmount { get; set; }
        public long settlementAmount { get; set; }
        public long wageAmount { get; set; }
        public string settlementIban { get; set; }
        public List<ScatteredSettlementDetails> scatteredSettlement { get; set; }
    }
}
