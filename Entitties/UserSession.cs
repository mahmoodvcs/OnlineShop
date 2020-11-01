using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    public class UserSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long UserId { get; set; }
        public User User { get; set; }
        public DateTime LoginDate { get; set; }
        public DateTime LastActivityDate { get; set; }
        public string IPAddress { get; set; }
        public string Token { get; set; }
    }
}
