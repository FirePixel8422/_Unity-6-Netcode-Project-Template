using UnityEngine;

public abstract class MultiInstanceBehaviour<T> : MonoBehaviour where T : MultiInstanceBehaviour<T>
{
    [SerializeField] private int id;
    public int Id => id;
    public static T[] Instances { get; private set; }

    protected virtual void Awake()
    {
        if (Instances == null)
        {
            Instances = new T[id + 1];
        }
        else if (id >= Instances.Length)
        {
            T[] instancesRef = Instances;
            System.Array.Resize(ref instancesRef, id + 1);
            Instances = instancesRef;
        }

        Instances[id] = (T)this;
    }
}
