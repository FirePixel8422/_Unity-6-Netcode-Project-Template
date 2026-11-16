using UnityEngine;

namespace CrowGroup.Utility.Scriptable
{
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "ScriptableObjects/SOVariables/FloatVariable", order = -799)]
    public class FloatVariableSO : VariableSO<float>, INumericalVariable<float>
    {
        public void Add(float value)
        {
            CurrentValue += value;
        }
    }
}