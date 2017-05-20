using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Data.Entity
{
	/// <summary>
	/// Helps sort a query by 1 or more columns without having to consider when OrderBy() vs ThenBy() would be best.
	/// </summary>
	public class EFSorter
	{
		public static EFSorter<TModel> Create<TModel>(IQueryable<TModel> unsorted)
		{
			return new EFSorter<TModel>(unsorted);
		}
	}
	public class EFSorter<TModel> : EFSorter
	{
		protected IQueryable<TModel> unsorted;
		protected IOrderedQueryable<TModel> sorted;

		public EFSorter(IQueryable<TModel> unsorted)
		{
			this.unsorted = unsorted;
		}

		public EFSorter<TModel> SortBy<TCol>(Expression<Func<TModel, TCol>> col, bool isDesc = false)
		{
			if (sorted == null)
			{
				sorted = isDesc ? unsorted.OrderByDescending(col) : unsorted.OrderBy(col);
				unsorted = null;
			}
			else
			{
				sorted = isDesc ? sorted.ThenByDescending(col) : sorted.ThenBy(col);
			}

			return this;
		}

		public EFDirectionedSorter<TModel> Direction(bool isDesc)
		{
			return new EFDirectionedSorter<TModel>(isDesc, this);
		}

		public IOrderedQueryable<TModel> Sorted
		{
			get
			{
				return sorted;
			}
		}
	}

	public class EFDirectionedSorter<TModel>
	{
		protected bool isDesc;
		protected EFSorter<TModel> sorter;

		public EFDirectionedSorter(bool isDesc, EFSorter<TModel> sorter)
		{
			this.isDesc = isDesc;
			this.sorter = sorter;
		}

		public bool SortBy<TCol>(Expression<Func<TModel, TCol>> col)
		{
			sorter.SortBy<TCol>(col, isDesc);
			return isDesc;
		}
	}
}
