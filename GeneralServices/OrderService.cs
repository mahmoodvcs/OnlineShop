using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.GeneralServices
{
    public class OrderService
    {
        private readonly DataContext dataContext;

        public OrderService(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }


    }
}
