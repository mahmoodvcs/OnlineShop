using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.Exceptions
{
    public class EntityNotFoundException<TEntity> : ApiException
    {
        public EntityNotFoundException(long id) : base(400, $"{DataContext.GetEntityTitle<TEntity>()} با کد {id} پیدا نشد")
        {
        }
    }
}
