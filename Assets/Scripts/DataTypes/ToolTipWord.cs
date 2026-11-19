using UnityEngine;

[System.Serializable]
public struct ToolTipWord
{
    [field: SerializeField] public string Word { get; private set; }
    [field: SerializeField] public Color Color { get; private set; }

    [field: TextArea]
    [field: SerializeField] public string ToolTip { get; private set; }

    [field: Header("Layout")]
    [field: SerializeField] public float Width { get; private set; }
    [field: SerializeField] public float Height { get; private set; }
}