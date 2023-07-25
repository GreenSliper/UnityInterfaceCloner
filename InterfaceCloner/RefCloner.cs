using InterfaceCloner.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace InterfaceCloner
{
	public class RefCloner : IRefCloner
	{
		TransformFinder transformFinder = new TransformFinder();

		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;

		Dictionary<Type, GenericCloner> genericCloners = new Dictionary<Type, GenericCloner>();
		
		/// <summary>
		/// Create reference cloner
		/// </summary>
		/// <param name="genericCloners">Implement GenericCloner to convert custom generics && unsupported types</param>
		public RefCloner(IEnumerable<GenericCloner> genericCloners = null) 
		{ 
			if(genericCloners != null)
				foreach(var cloner in genericCloners)
					this.genericCloners.Add(cloner.TypeDefinition, cloner);
		}

		public void FixRefernces(Transform target, Transform source, bool processChildren = true)
		{
			var monos = CollectMono(source, processChildren);
			foreach (var mono in monos)
			{
				var clonedMono = GetClonedVersion(source, target, mono);
				if (clonedMono == null)
				{
					Debug.LogWarning("Failed to find cloned copy of source mono marked by FixClonedAttribute");
					continue;
				}
				var fields = mono.GetType().GetFields(flags);
				foreach (var field in fields)
				{
					var attr = field.GetCustomAttribute<FixRefAttribute>();
					if(attr == null)
						continue;
					//correctly get a copy of field
					object restoredValue = null;
					if (field.FieldType.IsArray && field.FieldType.GetElementType().IsInterface)
						restoredValue = CloneArray(source.transform, target.transform, mono, field, attr);
					else if (field.FieldType.IsGenericType)
						restoredValue = CloneGeneric(source.transform, target.transform, mono, field, attr);
					else if (!field.FieldType.IsInterface)
					{
						Debug.LogWarning("Used FixRefAttribute on non-interface field. Ignored.");
						continue;
					}
					else
						restoredValue = CloneField(source.transform, target.transform, mono, field, attr);
					//set relative field value
					if (restoredValue != null)
						field.SetValue(clonedMono, restoredValue);
				}
			}
		}

		/// <summary>
		/// Restore references to array elements
		/// </summary>
		object CloneArray(Transform srcRoot, Transform cloneRoot, MonoBehaviour monoSrc, FieldInfo srcField, FixRefAttribute refAttribute)
		{
			if (TryCloneRefGetValue(monoSrc, srcField, refAttribute, out var srcVal))
				return srcVal;
			var rank = srcField.FieldType.GetArrayRank();
			if (rank != 1)
			{
				Debug.LogWarning("Multidimensional arrays not supported");
			}
			var srcArr = srcVal as Array;
			var result = Array.CreateInstance(srcField.FieldType.GetElementType(), srcArr.Length);

			int cnter = 0;
			foreach (var elem in srcArr)
				result.SetValue(CloneFieldValue(srcRoot, cloneRoot, elem, refAttribute), cnter++);
			return result;
		}

		/// <summary>
		/// Restore references to generic interface underlying element
		/// </summary>
		object CloneGeneric(Transform srcRoot, Transform cloneRoot, MonoBehaviour monoSrc, FieldInfo srcField, FixRefAttribute refAttribute)
		{
			if (TryCloneRefGetValue(monoSrc, srcField, refAttribute, out var srcVal))
				return srcVal;
			var srcType = srcVal.GetType();
			var genericDef = srcType.GetGenericTypeDefinition();
			if (genericCloners.TryGetValue(genericDef, out var cloner))
			{
				var values = cloner.GetOriginalValues(srcVal)
					//clone values properly
					.Select(x => CloneFieldValue(srcRoot, cloneRoot, x, refAttribute));
				return cloner.CreateInstance(srcType.GetGenericArguments(), values);
			}
			//list
			/*if (srcType.GetGenericTypeDefinition() == typeof(List<>))
			{
				var listType = typeof(List<>).MakeGenericType(srcType.GetGenericArguments());
				IList result = Activator.CreateInstance(listType) as IList;
				foreach (var item in srcVal as IList)
					result.Add(CloneFieldValue(srcRoot, cloneRoot, item, refAttribute));
				return result;
			}*/
			//add other generic types here
			Debug.LogWarning("Type not supported!");
			return null;
		}

		object CloneField(Transform srcRoot, Transform cloneRoot, MonoBehaviour monoSrc, FieldInfo srcField, FixRefAttribute refAttribute)
		{
			if (TryCloneRefGetValue(monoSrc, srcField, refAttribute, out var srcVal))
				return srcVal;
			return CloneFieldValue(srcRoot, cloneRoot, srcVal, refAttribute);
		}

		object CloneFieldValue(Transform srcRoot, Transform cloneRoot, object srcVal, FixRefAttribute refAttribute)
		{
			if (refAttribute.ICloneableRef)
				if (srcVal is ICloneable icl)
					return icl.Clone();
				else
				{
					Debug.LogError("Field marked as ICloneable reference does not contain ICloneable underlying type");
					return null;
				}
			//restore references to components
			if (refAttribute.IsChildComponentRef)
			{
				if (srcVal is MonoBehaviour mono)
				{
					var clonedMono = GetClonedVersion(srcRoot, cloneRoot, mono);
					if (clonedMono == null)
					{
						Debug.LogWarning("Corresponding mono not found in cloned hierarchy! You can use passReferenceOnly to pass reference to shared mono");
						return srcVal;
					}
					return clonedMono;
				}
				else
					Debug.LogError("Non-mono object found in field marked as childComponentRef. " +
						"Note that the given mono should be descendant of the root clone object");
			}
			Debug.LogWarning("Unsupported flag combination");
			return null;
		}

		/// <summary>
		/// Returns true if copy operation was completed
		/// </summary>
		/// <param name="fieldValue">Returns field value</param>
		bool TryCloneRefGetValue(MonoBehaviour monoSrc, FieldInfo srcField, FixRefAttribute refAttribute, out object fieldValue)
		{
			fieldValue = srcField.GetValue(monoSrc);
			if (fieldValue == null)
				return true;
			//simply pass reference
			if (refAttribute.PassReferenceOnly)
				return true;
			return false;
		}

		MonoBehaviour GetClonedVersion(Transform sourceObject, Transform clonedObject, MonoBehaviour sourceMono)
		{
			var clonedTransform = transformFinder.FindCloned(sourceObject, clonedObject, sourceMono.transform);
			if (clonedTransform == null)
				return null;
			MonoBehaviour mono = clonedTransform.GetComponent(sourceMono.GetType()) as MonoBehaviour;
			if (mono == null)
				Debug.LogWarning("Could not find component of same type in the cloned hierarchy!");
			return mono;
		}

		IEnumerable<MonoBehaviour> CollectMono(Transform target, bool addChildren = true)
		{
			MonoBehaviour[] components = null;
			if(addChildren)
				components = target.GetComponentsInChildren<MonoBehaviour>();
			else
				components = target.GetComponents<MonoBehaviour>();
			return components.Where(x => Attribute.GetCustomAttribute(x.GetType(), typeof(FixClonedAttribute)) != null);
		}
	}
}
