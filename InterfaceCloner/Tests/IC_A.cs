using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InterfaceCloner.Tests
{
    [Serializable]
    public class IC_InnerClass : ICloneable
    {
		[FixRef(childComponentRef: true)]
		public IIC_B innerComponent;

        public object Clone() => MemberwiseClone();
	}

	[FixCloned]
    public class IC_A : MonoBehaviour
    {
        [FixRef(icloneableRef:true)]
        ICloneable cl = new IC_Cloneable();
        [FixRef(childComponentRef: true)]
        IIC_B childComponentInterface;
        [FixRef(fixInnerType: true)]
        IC_InnerClass innerClassRef;
		[FixRef(childComponentRef: true)]
        IIC_B[] childComponentArray;
		[FixRef(childComponentRef: true)]
		List<IIC_B> childComponentList;

		[SerializeField]
        Transform[] iic_bArray;

        [SerializeField]
		bool check = false, init = true;

        // Update is called once per frame
        void Update()
        {
            if (check)
            {
                CheckInterfaceState();
                check = false;
            }
            if (!init)
            {
                Init();
				init = true;
            }
        }

        void Init()
        {
            childComponentInterface = GetComponentInChildren<IIC_B>();
            innerClassRef = new IC_InnerClass() { innerComponent = childComponentInterface };
			childComponentArray = new IIC_B[iic_bArray.Length];
            for(int i = 0; i < iic_bArray.Length; i++)
                childComponentArray[i] = iic_bArray[i].GetComponent<IIC_B>();
            childComponentList = new List<IIC_B>();
			childComponentList.AddRange(childComponentArray);
		}

        void CheckInterfaceState()
        {
            if (cl != null)
				if (cl is IC_Cloneable icc)
					Debug.Log(icc.cnt);
            else 
			{
                Debug.LogError("ICloneable lost");
                return;
            }
            
            if (childComponentInterface as MonoBehaviour != null)
                childComponentInterface.PrintField();
            else
            {
                Debug.LogError("Component interface field lost");
                return;
            }

            if (innerClassRef != null && innerClassRef.innerComponent != null)
                innerClassRef.innerComponent.PrintField();
            else
                Debug.LogError("Inner class reference lost");

			if (childComponentArray != null)
                Debug.Log($"Array: {string.Join(", ", childComponentArray.Select(x => x.Field))}");
            else
            {
                Debug.LogError("Array lost");
                return;
            }

            if (childComponentList != null)
                Debug.Log($"List: {string.Join(", ", childComponentList.Select(x => x.Field))}");
            else
            {
                Debug.LogError("List lost");
                return;
            }

            Debug.Log("OK");
        }
    }

	public class IC_Cloneable : ICloneable
	{
        public static int cnter = 0;

        public int cnt = 0;

        public IC_Cloneable()
        {
            cnter++;
            cnt = cnter;
        }

		public object Clone()
		{
            return new IC_Cloneable();
		}
	}
}