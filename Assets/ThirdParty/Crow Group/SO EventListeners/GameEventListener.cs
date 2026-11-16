using System;
using UnityEngine;
using UnityEngine.Events;

namespace CrowSupport.Events
{
    [Serializable]
    public class GameEventListener : EventListenerBase
    {
        [SerializeField] protected GameEvent _event;
        [SerializeField] protected UnityEvent _response;

        protected virtual void OnEnable()
        {
            _event?.AddListener(OnEventInvoke);
            DebugLogger.LogError($"Event listener on \"{name}\" does not have a event to listen to selected", _event == null);
        }
        protected virtual void OnDisable()
        {
            _event?.RemoveListener(OnEventInvoke);
            DebugLogger.LogError($"Event listener \"{name}\" does not have a event to listen to selected", _event == null);
        }
        public virtual void OnEventInvoke()
        {
            _response?.Invoke();
            DebugLogger.LogError($"Event listener \"{name}\" does not have a response to call when an event is fired", _response == null);
        }
    }
}