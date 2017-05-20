using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Reflection;


namespace Brass9.Data.Entity
{
	public static class EFMetaHelper
	{
		/// <summary>
		/// Gets the hidden ObjectContext that backs this DbContext.
		/// http://social.msdn.microsoft.com/Forums/en-US/0c1f8425-55f4-4600-9605-a925220d5a25/objectcontext-from-dbcontext?forum=adonetefx
		/// </summary>
		/// <param name="db"></param>
		/// <returns></returns>
		public static ObjectContext ObjectContextForDb(DbContext db)
		{
			return ((IObjectContextAdapter)db).ObjectContext;
		}

		/// <summary>
		/// Gets the Keys for a given Entity model, as PropertyInfo objects
		/// </summary>
		/// <param name="o"></param>
		/// <param name="db"></param>
		/// <returns></returns>
		public static IEnumerable<PropertyInfo> GetKeysForModel(object o, DbContext db)
		{
			return GetKeysForModelType(o.GetType(), db);
		}

		/// <summary>
		/// Gets the keys for a given Entity model Type, as PropertyInfo objects
		/// 
		/// Extracted from EF GraphDiff, RefactorThis.GraphDiff.Internal.Extensions.GetPrimaryKeyFieldsFor
		/// </summary>
		/// <param name="type"></param>
		/// <param name="db"></param>
		/// <returns></returns>
		public static IEnumerable<PropertyInfo> GetKeysForModelType(Type type, DbContext db)
		{
			var metadataWorkspace = ObjectContextForDb(db).MetadataWorkspace;
			var metadata = metadataWorkspace.GetItems<EntityType>(DataSpace.OSpace)
					.SingleOrDefault(p => p.FullName == type.FullName);
			return metadata.KeyMembers
				.Select(k => type.GetProperty(k.Name,
					BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
					.ToList();
		}

		/// <summary>
		/// Ambitiously assumes the passed in object is an Entity, and there's exactly one key on
		/// the model. Returns the value of that key.
		/// </summary>
		/// <param name="o"></param>
		/// <param name="db"></param>
		/// <returns></returns>
		public static object GetKey(object o, DbContext db)
		{
			var keys = GetKeysForModel(o, db);
			return keys.First().GetValue(o);
		}
	}
}
