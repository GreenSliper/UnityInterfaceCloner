using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InterfaceCloner.Tests
{
    public class IC_Cloner : MonoBehaviour
    {
		IRefCloner cloner = new RefCloner(new GenericCloner[] {
					new ListCloner() //generics support
                });
        [SerializeField]
        Transform cloneNext, cloneParent;
		[SerializeField]
		bool fixReferences = true;
		[SerializeField]
        bool clone = false;

		private void Update()
		{
			if (clone)
			{
				var cloneTr = Instantiate(cloneNext, cloneParent);
				if(fixReferences)
					cloner.FixRefernces(cloneTr, cloneNext, true);
				clone = false;
			}
		}
	}
}