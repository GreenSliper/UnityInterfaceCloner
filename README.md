# UnityInterfaceCloner
Unity (at least version 2019.4.36f1 and earlier) cannot serialize interface fields, so after instantiation all references to MonoBehaviours hidden by interfaces. The same situation is with interface arrays and generic containers. 

For example, we create a component IC_A:

```csharp 
public class IC_A : MonoBehaviour
{
    IIC_B childComponentInterface;
}
```

And child component IC_B with interface IIC_B:

```csharp 
public interface IIC_B
  {
      void Print();
  }

public class IC_B : MonoBehaviour, IIC_B
{
  public void Print()
  {
    Debug.Log(0);
  }
}
```

If we clone object with IC_A via MonoBehaviour.Instantiate, the reference in the field IIC_B childComponentInterface will be lost.

InterfaceCloner can help to solve the problem. Here is how:
Use ```[FixCloned]``` attribute to mark monos needing reference fixes.
Use ```[FixRef]``` attribute to mark fields for refence fixing.

```csharp
 [FixCloned]
  public class IC_A : MonoBehaviour
  {
    [FixRef]
    ICloneable cl = new IC_Cloneable();
    // next fields hide components, which are attached to the same gameobject or one of its' descendants.
    // This flag marks they are cloned with the base object, and the values should be found relatively to the created object
    [FixRef(childComponentRef: true)]
    IIC_B childComponentInterface;
    [FixRef(childComponentRef: true)]
    IIC_B[] childComponentArray;
    // ListCloner is included: other generics' support can be added. Check Generics/ListCloner.cs 
    [FixRef(childComponentRef: true)]
    List<IIC_B> childComponentList;
  }
```

Restore references to:
 - nested gameobjects/components in interface fields (use ```FixRef(childComponentRef: true)```)
 - ICloneable references with FixRefAttribute & ICloneableRef flag are cloned via interface 
 - other interface fields with FixRefAttribute are filled with references on existing objects (without cloning)

Support for various generics can be added by implementing ```abstract class GenericCloner``` and using it when creating RefCloner instance.

Test scene can be found in Tests folder.
