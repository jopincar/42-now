using System;
using System.Linq;
using System.Linq.Expressions;

namespace FortyTwoAccess
{
	public static class DynamicOrderBy
	{
		public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string orderBy) where TEntity : class
		{
			var ret = source;
			orderBy = orderBy.Replace(", ", ",").Replace(" ,", ",").Replace(" , ", ",");
			var colspecs = orderBy.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = colspecs.Length - 1; i >= 0; i--)
			{
				var tokens = colspecs[i].Split(new char[] { ' ' });
				bool desc = tokens.Length > 1 && tokens[1].ToLower().Equals("desc");
				ret = ret.OrderBy(tokens[0], desc);
			}
			return ret;
		}

		public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string orderByProperty,
			bool desc) where TEntity : class
		{
			string command = desc ? "OrderByDescending" : "OrderBy";
			var type = typeof(TEntity);
			var property = type.GetProperty(orderByProperty);
			var parameter = Expression.Parameter(type, "p");
			var propertyAccess = Expression.MakeMemberAccess(parameter, property);
			var orderByExpression = Expression.Lambda(propertyAccess, parameter);
			var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType },
				source.Expression, Expression.Quote(orderByExpression));
			return source.Provider.CreateQuery<TEntity>(resultExpression);
		}
	}
}