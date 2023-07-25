using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEditor;
using UnityEngine;

namespace InterfaceCloner
{
	public class ListCloner : GenericCloner
	{
		public ListCloner() : base()
		{
			TypeDefinition = typeof(List<>);
		}

		public override object CreateInstance(Type[] genericArguments, IEnumerable<object> enumerableValues)
		{
			var listType = typeof(List<>).MakeGenericType(genericArguments);
			IList result = Activator.CreateInstance(listType) as IList;
			foreach (var item in enumerableValues)
				result.Add(item);
			return result;
		}

		public override IEnumerable<object> GetOriginalValues(object src)
		{
			foreach (var val in (src as IList))
				yield return val;
			//return (src as IList).Cast<object>();
		}
	}
}