using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MahtaKala.Entities.Extentions
{
    public interface IPagingData
    {
        int Offset { get; set; }
        int Page { get; set; }
    }

    public static class EntityExtentions
    {
        public static IQueryable<T> Page<T>(this IQueryable<T> query, IPagingData paging)
        {
            return query.Skip(paging.Offset).Take(paging.Page);
        }

    }
}
