using MahtaKala.Entities.Security;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahtaKala.Entities
{
    public partial class User
    {
        public User()
        {
            Tokens = new List<UserToken>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [StringLength(255)]
        public string Username { get; set; }
        public string Password { get; set; }
        public string SecurityStamp { get; set; }
        [StringLength(255)]
        public string Name { get; set; }
        [StringLength(255)]
        public string EmailAddress { get; set; }
        [StringLength(255)]
        public string MobileNumber { get; set; }
        public UserType Type { get; set; }

        public ICollection<UserToken> Tokens { get; set; }


        public static User Create(
            string username,
            string password,
            string name,
            string email,
            UserType type)
        {
            return new User
            {
                Username = username,
                Password = PasswordHasher.Hash(password),
                SecurityStamp = Security.SecurityStamp.Generate(),
                Name = name,
                EmailAddress = email,
                Type = type,
            };
        }
        public static User Create(
            string mobile,
            UserType type)
        {
            return new User
            {
                SecurityStamp = Security.SecurityStamp.Generate(),
                MobileNumber = mobile,
                Type = type,
            };
        }
    }
}