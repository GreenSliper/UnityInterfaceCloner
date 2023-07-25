using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InterfaceCloner.Tools
{
    public class TransformFinder
    {
        public Transform FindCloned(Transform srcRoot, Transform cloneRoot, Transform targetInSrc)
        {
            if (targetInSrc == srcRoot)
                return cloneRoot;
            //collect path
            List<int> path = new List<int>();
            Transform temp = targetInSrc;
            while (temp != null && temp != srcRoot)
            {
                path.Add(temp.GetSiblingIndex());
                temp = temp.parent;
            }
            if (temp == null)
                Debug.LogError("Error inspecting hierarchy: targetInSrc is not descendant of srcRoot");
            //restore path
            temp = cloneRoot;
            for (int i = path.Count - 1; i >= 0; i--)
            {
                if (temp.childCount < path[i])
                {
                    Debug.LogWarning("Failed to restore path to cloned descendant transform!");
                    return null;
                }
                temp = temp.GetChild(path[i]);
            }
            return temp;
        }
    }
}