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

    public static class CategoryExtensions
    {
        public static void GetCategoryChildrenRecursive(this Category category, List<long> resultList)
        {
            if (category.Children == null || category.Children.Count == 0)
                return;
            foreach (var child in category.Children)
            {
                if (!resultList.Contains(child.Id))
                {
                    resultList.Add(child.Id);
                    child.GetCategoryChildrenRecursive(resultList);
                }
            }
        }
    }
}
