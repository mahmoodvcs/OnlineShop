using MahtaKala.Entities.Security;
using System;
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
        [Display(Name = "نام کاربری")]
        public string Username { get; set; }
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }
        [StringLength(255)]
        [Display(Name = "نام")]
        public string FirstName { get; set; }
        [StringLength(255)]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }
        [StringLength(10)]
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        public string SecurityStamp { get; set; }
        [StringLength(255)]
        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage ="آدرس ایمیل معتبر نیست")]
        public string EmailAddress { get; set; }
        [StringLength(255)]
        [Display(Name = "شماره همراه")]
        public string MobileNumber { get; set; }
        [Display(Name = "نوع کاربر")]
        public UserType Type { get; set; }


        public string FullName()
        {
            string name = FirstName + " " + LastName;
            if (string.IsNullOrEmpty(name.Trim()))
                name = Username;
            return name;
        }
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

        public void CheckProfileCompletion()
        {
            var msg = "لطفا به صفحه ی پروفایل کاربری رفته و {0} خود را وارد کنید";
            if (string.IsNullOrEmpty(FirstName))
                throw new Exception(string.Format(msg, "نام"));
            if (string.IsNullOrEmpty(LastName))
                throw new Exception(string.Format(msg, "نام خانوادگی"));
            if (string.IsNullOrEmpty(NationalCode))
                throw new Exception(string.Format(msg, "کد ملی"));
        }
    }
}