using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;

using Brass9.Collections.Linq;


namespace Brass9.Data.Entity.IQueryableExtensions
{
	public static class IQueryableExtension
	{
		public static async Task<HashSet<T>> ToHashSetAsync<T>(this IQueryable<T> query)
		{
			var arr = await query.ToArrayAsync();
			return arr.ToHashSet();
		}

		public static async Task<HashSet<TKey>> ToHashSetAsync<TSource, TKey>(this IQueryable<TSource> query, Expression<Func<TSource, TKey>> selectKey)
		{
			var map = new HashSet<TKey>();
			await query.Select(selectKey).ForEachAsync(k => map.Add(k));
			return map;
		}

		public static async Task<HashSet<T>> ToHashSetIgnoreNullAsync<T>(this IQueryable<Nullable<T>> query)
			where T : struct
		{
			HashSet<T> map = new HashSet<T>();
			await query.Where(x => x.HasValue).ForEachAsync(x => map.Add(x.Value));
			return map;
		}
	}
}
