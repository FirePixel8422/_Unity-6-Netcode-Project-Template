using System;
using UnityEngine;

namespace CrowGroup.Utility.Scriptable
{
    public class VariableSO<T> : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, TextArea] private string _developerNotes;
#endif

        [SerializeField] private T _startValue;
        [SerializeField] private T _currentValue;

        public Action<T> OnValueChanged;

        public T CurrentValue
        {
            get => _currentValue;
            set
            {
                OnValueChanged?.Invoke(value);
                _currentValue = value;
            }
        }
        public T SilentCurrentValue
        {
            get => _currentValue;
            set => _currentValue = value;
        }

        private void OnEnable()
        {
            CurrentValue = _startValue;
        }
    }

    public interface INumericalVariable<T>
    {
        public void Add(T value);
    }
}
