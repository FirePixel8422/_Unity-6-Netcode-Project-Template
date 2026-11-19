using UnityEngine;

[CreateAssetMenu(fileName = "New ToolTips list", menuName = "ScriptableObjects/Misc/ToolTipsSO", order = -100)]
public class ToolTipsSO : ScriptableObject
{
    [field: SerializeField] public ToolTipWord[] Data { get; private set; }
}