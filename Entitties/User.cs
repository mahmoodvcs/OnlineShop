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
            RefreshTokens = new List<RefreshToken>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [StringLength(255)]
        public string Username { get; set; }
        public string Password { get; set; }
        [StringLength(255)]
        public string FirstName { get; set; }
        [StringLength(255)]
        public string LastName { get; set; }
        [StringLength(10)]
        public string NationalCode { get; set; }

        public string SecurityStamp { get; set; }
        [StringLength(255)]
        [EmailAddress(ErrorMessage ="آدرس ایمیل معتبر نیست")]
        public string EmailAddress { get; set; }
        [StringLength(255)]
        public string MobileNumber { get; set; }
        public UserType Type { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; }


        public static User Create(
            string username,
            string password,
            string email,
            UserType type)
        {
            return new User
            {
                Username = username,
                Password = PasswordHasher.Hash(password, ((int)type).ToString()),
                SecurityStamp = Security.SecurityStamp.Generate(),
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

        public bool VerifyPassword(string password)
        {
            return PasswordHasher.Verify(this.Password, password, ((int)this.Type).ToString());
        }
    }
}