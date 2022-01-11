using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class QueryableExtensions
    {
        /*
            Orders the current IQueryable instance in descending based on the given string.
            The given string represents a attribute (column) of <T> which is a model (table).
            Credits to: https://stackoverflow.com/questions/307512/how-do-i-apply-orderby-on-an-iqueryable-using-a-string-column-name-within-a-gene
        */
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string columnToOrderWith, bool ascending = true)
        {
            var type = typeof(T);    
            var propName = columnToOrderWith;
            // Get the property that matches with the given property name
            var property = type
                        .GetProperties()
                        .SingleOrDefault(x => x.Name.Equals(propName,StringComparison.OrdinalIgnoreCase));
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            var methodName = (ascending ? "OrderBy" : "OrderByDescending");
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), methodName, new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExp));
            return source.Provider.CreateQuery<T>(resultExp);
        }
    }
}