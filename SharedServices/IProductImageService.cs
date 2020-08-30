using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.SharedServices
{
    public interface IProductImageService
    {
        string GetImageUrl(long productId, string name);

    }

}
