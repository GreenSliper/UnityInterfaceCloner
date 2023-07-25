using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InterfaceCloner
{
    /// <summary>
    /// Use to create custom type cloners for RefCloner. To be used with generic containers like List, HashSet, Dictionary, etc.
    /// </summary>
    public abstract class GenericCloner
    {
        public Type TypeDefinition { get; protected set; }
        /// <summary>
        /// Use Type.GetGenericTypeDefinition for generic types
        /// </summary>
        /// <param name="typeDefinition"></param>
        public GenericCloner(Type typeDefinition) 
        {
            this.TypeDefinition = typeDefinition;
        }

        /// <summary>
        /// Use to init typeDefinition manually
        /// </summary>
        protected GenericCloner()
        { }

        /// <summary>
        /// Cloned values passed only
        /// </summary>
        public abstract object CreateInstance(Type[] genericArguments, IEnumerable<object> clonedValues);
        public abstract IEnumerable<object> GetOriginalValues(object src);
    }
}