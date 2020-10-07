using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.Exceptions
{
    public class CartItemException : ApiException
    {
        public CartItemException(string message, long productId, string productName) : base(400, message)
        {
            ProductName = productName;
            ProductId = productId;
        }

        public string ProductName { get; set; }
        public long ProductId { get; set; }
    }
}
