using UnityEngine;

namespace CrowGroup.Utility.Scriptable
{
    [CreateAssetMenu(fileName = "IntVariable", menuName = "ScriptableObjects/SOVariables/IntVariable", order = -800)]
    public class IntVariableSO : VariableSO<int>, INumericalVariable<int>
    {
        public void Add(int value)
        {
            CurrentValue += value;
        }
    }
}
