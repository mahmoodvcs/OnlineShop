using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.SharedServices
{
    public interface IPathService
    {
        string AppRoot { get; }
        string AppBaseUrl { get; }
    }
}
