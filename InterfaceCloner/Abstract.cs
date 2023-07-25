using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InterfaceCloner
{
    public interface IRefCloner
    {
		/// <summary>
		/// Restore references to:
		/// - nested gameobjects/components in interface fields
		/// - ICloneable references with FixRefAttribute & ICloneableRef flag are cloned via interface
		/// - other interface fields with FixRefAttribute are filled with references on existing objects
		/// </summary>
		/// <param name="target">Gameobject with lost references (ex. cloned)</param>
		/// <param name="source">Gameobject with source components</param>
		/// <param name="processChildren">Restore refs on child gameobjects</param>
		void FixRefernces(Transform target, Transform source, bool processChildren = true);
    }

    /// <summary>
    /// Use on components which interface fields are lost
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class FixClonedAttribute : Attribute
    {
        public FixClonedAttribute() 
        { 
        }
    }

    /// <summary>
    /// Use on interface fields lost after cloning
    /// </summary>
	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public class FixRefAttribute : Attribute
	{
		public bool ICloneableRef => _icloneableRef;
		public bool IsChildComponentRef => _childComponentRef;
		public bool PassReferenceOnly => _passReferenceOnly;

		bool _icloneableRef, _childComponentRef, _passReferenceOnly;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="icloneableRef">Use on interface fields which hidden types also implement ICloneable interface. 
        /// Otherwise the reference is copied</param>
		public FixRefAttribute(bool icloneableRef = false, bool childComponentRef = false, bool passReferenceOnly = false)
		{
            _icloneableRef = icloneableRef;
			_childComponentRef = childComponentRef;
			_passReferenceOnly = passReferenceOnly;
		}
	}
}