using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Brass9.Reflection;


namespace Brass9.Data
{
	public class ModelTransformInherit
	{
		public static void Copy(object from1, Type from1Type, object from2, Type from2Type, object to, Type toType, string[] skipProperties, Action<object, PropertyInfo> copyAction)
		{
			Func<PropertyInfo, bool> where = null;
			if (skipProperties != null)
				where = p => !skipProperties.Contains(p.Name);

			ModelTransform.Copy(from1, from1Type, to, toType, where, (name, from1Val) =>
			{
				if (from1Val == InheritValue(from1Val))
					return ReflectionHelper.GetPropValue(from2, name);

				return from1Val;
			});
		}

		public const decimal InheritDecimal = decimal.MinValue;

		public static object InheritValue(decimal d)
		{
			return InheritDecimal;
		}

		public static object InheritValue(object o)
		{
			return null;
		}
	}
}
