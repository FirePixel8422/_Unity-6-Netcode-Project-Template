using Fire_Pixel.Utility;
using UnityEngine;

public class TransformFollow : UpdateMonoBehaviour
{
    [ErrorIf(ComparisonType.IsNull, "Please assign a target transform.")]
    [SerializeField] private Transform _target;
    [SerializeField] private float lerpSpeed = 2;

    [SerializeField, InspectorReadOnly] private Vector3 _position;
    [SerializeField, InspectorReadOnly] private Quaternion _rotation;


    private void Awake()
    {
        _target.GetPositionAndRotation(out _position, out _rotation);
    }

    protected override void OnUpdate()
    {
        float t = lerpSpeed * Time.deltaTime;
        _position = Vector3.Lerp(_position, _target.position, t);
        _rotation = Quaternion.Lerp(_rotation, _target.rotation, t);

        transform.SetPositionAndRotation(_position, _rotation);
    }
}
