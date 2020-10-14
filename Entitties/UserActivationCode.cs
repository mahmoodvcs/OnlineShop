using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    public class UserActivationCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public int Code { get; set; }
        public DateTime IssueTime { get; set; }
        public DateTime ExpireTime { get; set; }
		public string AdditionalData { get; set; }
	}
}
