using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    [Table(name: "received_sms")]
    public class ReceivedSMS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [StringLength(20)]
        public string Sender { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public string OperatorId { get; set; }
    }
}
