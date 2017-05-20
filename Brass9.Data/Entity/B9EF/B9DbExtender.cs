using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brass9.Collections.HasProp;
using Brass9.Reflection;

namespace Brass9.Data.Entity.B9EF
{
	public class B9DbExtender
	{
		public static B9DbExtender New() { return new B9DbExtender(); }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="modelBuilder"></param>
		/// <param name="modelsProject">Like var modelsProject = System.Reflection.Assembly.GetExecutingAssembly();</param>
		public void Extend(DbModelBuilder modelBuilder, System.Reflection.Assembly modelsProject)
		{
			var applyTablePerConcreteMethod = new Action<DbModelBuilder>(applyTablePerConcrete<object>).Method.GetGenericMethodDefinition();
			var applyForcePKIdMethod = new Action<DbModelBuilder>(applyForcePKId<IId>).Method.GetGenericMethodDefinition();

			AttributeHelper.ForAllTypesWithAttribute<TablePerConcreteAttribute>(clas =>
			{
				var applyTPC = applyTablePerConcreteMethod.MakeGenericMethod(clas);
				applyTPC.Invoke(this, new object[] { modelBuilder });

				var applyPKId = applyForcePKIdMethod.MakeGenericMethod(clas);
				applyPKId.Invoke(this, new object[] { modelBuilder });
			}, null, modelsProject);
		}

		protected void applyTablePerConcrete<TTable>(DbModelBuilder modelBuilder)
			where TTable : class
		{
			string className = typeof(TTable).Name;
			modelBuilder.Entity<TTable>().Map(x => x.ToTable(className).MapInheritedProperties());
		}

		protected void applyForcePKId<TTable>(DbModelBuilder modelBuilder)
			where TTable : class, IId
		{
			modelBuilder.Entity<TTable>().HasKey(x => x.Id).Property<int>(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
		}
	}
}
