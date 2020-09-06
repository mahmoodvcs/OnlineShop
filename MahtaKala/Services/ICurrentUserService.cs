using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.SharedServices
{
    public interface ICurrentUserService
    {
        User User { get; }
        string AnonymousSessionId { get; }

        void RemoveCartCookie();
    }
}
