using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace MahtaKala.Entities
{
    [Owned]
    public class RefreshToken
    {
        [Key]
        [JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string Token { get; set; }
        public DateTime Expires { get; set; }
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created { get; set; }
        [StringLength(255)]
        public string CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        [StringLength(255)]
        public string RevokedByIp { get; set; }
        public string ReplacedByToken { get; set; }
        [NotMapped]
        public bool IsActive => Revoked == null && !IsExpired;
    }
}