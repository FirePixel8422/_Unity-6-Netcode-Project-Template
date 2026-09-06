using UnityEngine;


[System.Serializable]
public struct TransformOffset
{
    public Vector3 Position;
    public Vector3 EulerRotation;
    public Vector3 Scale;
    public readonly Quaternion Rotation => Quaternion.Euler(EulerRotation);


    public TransformOffset(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Position = position;
        EulerRotation = rotation;
        Scale = scale;
    }

    public static TransformOffset Default => new TransformOffset
    {
        Position = Vector3.zero,
        EulerRotation = Vector3.zero,
        Scale = Vector3.one,
    };
}