using System;

namespace MahtaKala.Entities
{
    public partial class UserToken
    {
        public int Id { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
        public string IpAddress { get; set; }
        public DateTime IssueTime { get; set; }
        public DateTime ExpireTime { get; set; }
        public string Token { get; set; }
        public bool Active { get; set; }


        public static UserToken Create(
            int userId,
            string ip,
            DateTime issueTime,
            DateTime expireTime,
            string tokenCipher)
        {
            return new UserToken
            {
                UserId = userId,
                IpAddress = ip,
                IssueTime = issueTime,
                ExpireTime = expireTime,
                Token = tokenCipher,
                Active = true
            };
        }
    }

}