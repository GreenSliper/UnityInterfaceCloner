using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InterfaceCloner.Tests
{
    public interface IIC_B
    {
        void PrintField();
		int Field { get; }
    }

	public class IC_B : MonoBehaviour, IIC_B
	{
		[SerializeField]
		int cnt = 0;

		public int Field => cnt;

		public void PrintField()
		{
			Debug.Log(cnt);
		}
	}
}