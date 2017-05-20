using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Brass9.Collections.HasProp;

namespace Brass9.Data.Entity.RuntimeModels
{
	public class RuntimeSelectBuilder
	{
		#region Singleton
		// http://codereview.stackexchange.com/questions/79/implementing-a-singleton-pattern-in-c
		public static RuntimeSelectBuilder O { get { return Nested.instance; } }

		class Nested
		{
			static Nested()
			{
			}

			internal static readonly RuntimeSelectBuilder instance = new RuntimeSelectBuilder();
		}
		#endregion

		public IQueryable<BaseRuntimeModel<T>> BuildSelect<T>(IQueryable<T> query, Expression<Func<T, IEnumerable>>[] includes)
			where T : class
		{
			var props = includes.Select(collExpr => (PropertyInfo)((MemberExpression)collExpr.Body).Member).ToArray();
			return BuildSelect(query, props);
		}

		public IQueryable<BaseRuntimeModel<T>> BuildSelect<T>(IQueryable<T> query, PropertyInfo[] includes)
			where T : class
		{
			// Generates the equivalent of
			//var props = new Dictionary<string, Type> { { "Videos", typeof(ICollection<Video>) }, { "WhatWeAreLearning", typeof(ICollection<WhatWeAreLearning>) } };
			var props = includes.ToDictionary(prop => prop.Name, prop => prop.PropertyType);
				
			var names = props.Select(d => d.Key).ToArray();

			// http://stackoverflow.com/a/723018/176877
			var runtimeSelectModelClass = RuntimeModelClassBuilder.O.GetRuntimeModel(props, typeof(BaseRuntimeModel<T>));

			var selectParam = Expression.Parameter(typeof(T), "m");
			var dbProps = query.ElementType.GetProperties().Where(p => props.ContainsKey(p.Name))
					.ToDictionary(p => p.Name);


			// 1) We filter the special property "Model" out, because it's not an ICollection
			var runtimeProps = runtimeSelectModelClass.GetProperties().Where(p => p.Name != "Model").ToArray();
			var bindingList = new List<MemberBinding>(runtimeProps.Length + 1);

			foreach(var prop in runtimeProps)
			{
				// Type inspection on the ICollection
				var propType = prop.PropertyType;

				if (typeof(IEnumerable).IsAssignableFrom(propType))
				{
					// Assume ICollection. TODO: Can we more accurately determine that?
					var itemType = propType.GetGenericArguments()[0];
					
					if (typeof(IOrderIndex).IsAssignableFrom(itemType))
					{
						var bindSetter = makeOrderByOrderIndexBinding(selectParam, prop, dbProps[prop.Name], itemType);
						bindingList.Add(bindSetter);
						continue;
					}
					else if(typeof(IId).IsAssignableFrom(itemType))
					{
						var bindSetter = makeOrderByIdBinding(selectParam, prop, dbProps[prop.Name], itemType);
						bindingList.Add(bindSetter);
						continue;
					}
				}
				// else

				var bind = Expression.Bind(prop.SetMethod,
					Expression.Property(selectParam, dbProps[prop.Name])
				);
				bindingList.Add(bind);
			}


			// 2) Fetch the root object as Model as a special assignment
			var modelProp = runtimeSelectModelClass.GetProperty("Model");
			// http://stackoverflow.com/a/15298301/176877
			var modelBinding = Expression.Bind(modelProp, selectParam);

			bindingList.Add(modelBinding);
			var bindings = bindingList.ToArray();

			var selectRuntime = Expression.Lambda(
				Expression.MemberInit(
					Expression.New(runtimeSelectModelClass.GetConstructor(Type.EmptyTypes)),
					bindings
				), selectParam
			);

			var queryWithSelect = query.Provider.CreateQuery(Expression.Call(
				typeof(Queryable), "Select", new Type[] { query.ElementType, runtimeSelectModelClass },
				Expression.Constant(query), selectRuntime
			));

			var _queryWithSelect = (IQueryable<BaseRuntimeModel<T>>)queryWithSelect;
			return _queryWithSelect;
		}

		protected MemberAssignment makeOrderByOrderIndexBinding(ParameterExpression selectParam, PropertyInfo runtimeProp, PropertyInfo dbProp, Type itemType)
		{
			return makeOrderByPropBinding("OrderIndex", selectParam, runtimeProp, dbProp, itemType);
		}

		protected MemberAssignment makeOrderByIdBinding(ParameterExpression selectParam, PropertyInfo runtimeProp, PropertyInfo dbProp, Type itemType)
		{
			return makeOrderByPropBinding("Id", selectParam, runtimeProp, dbProp, itemType);
		}

		/// <summary>
		/// Creates an Ordered select line, like
		/// .Select(m => new {
		///   Prop (begins here) = m.Prop.OrderBy(p => p.OrderIndex)
		///   
		/// The ordered prop must be an int, but this could easily be abstracted if ordering is needed for another
		/// data type.
		/// </summary>
		/// <param name="orderedPropName">Name of the ordered prop, like OrderIndex</param>
		/// <param name="selectParam">The ParameterExpression representing the opening of the Lambda, like m in m =></param>
		/// <param name="dbProp">The PropertyInfo for the Property that holds an IEnumerable to order</param>
		/// <param name="itemType">Type of the item in the IEnumerable, like Person</param>
		/// <returns>A MemberAssignment that can be passed to Expression.Bind to tie it to a Select Prop</returns>
		protected MemberAssignment makeOrderByPropBinding(string orderedPropName,
			ParameterExpression selectParam, PropertyInfo runtimeProp, PropertyInfo dbProp, Type itemType)
		{
			var propColl = Expression.Property(selectParam, dbProp);

			//http://stackoverflow.com/a/11337472/176877

			var orderByParam = Expression.Parameter(itemType, "p");
			var orderIndexProp = itemType.GetProperty(orderedPropName);

			var orderByExp = Expression.Lambda(
				Expression.MakeMemberAccess(orderByParam, orderIndexProp),
				orderByParam
			);

			var orderedProp = Expression.Call(
				// Call the OrderBy extension of the Queryable extension class
				typeof(Enumerable), "OrderBy",
				// Which takes 2 Generic arguments (usually inferred) - the IQueryable element type, and int since that's the type of OrderIndex
				new[] { itemType, typeof(int) },
				// The this argument
				propColl,
				// The Expression passed to the OrderBy call, like OrderBy( p => p.OrderIndex )
				//Expression.Quote(orderByExp)
				orderByExp
			);

			var bindSetter = Expression.Bind(runtimeProp.SetMethod, orderedProp);

			return bindSetter;
		}
	}
}
