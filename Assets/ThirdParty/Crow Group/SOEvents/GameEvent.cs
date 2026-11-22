using System;
using UnityEngine;

namespace CrowSupport.Events
{
    public abstract class GameEventBase : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, TextArea] private string _developerNotes;
#endif
    }


    [CreateAssetMenu(fileName = "GameEvent", menuName = "ScriptableObjects/SOEvents/GameEvent", order = -900)]
    public class GameEvent : GameEventBase
    {
        [Space(2)]
        [SerializeField] private Action _response;
        public bool HasListeners => _response != null;

        public virtual void Invoke()
        {
            DebugLogger.LogError($"Response is null for {name}", _response == null);

            _response.Invoke();
        }

        public void AddListener(Action method) => _response += method;
        public void RemoveListener(Action method) => _response -= method;

        public static GameEvent operator +(GameEvent gameEvent, Action method)
        {
            gameEvent.AddListener(method);
            return gameEvent;
        }
        public static GameEvent operator -(GameEvent gameEvent, Action method)
        {
            gameEvent.RemoveListener(method);
            return gameEvent;
        }
    }

    public class GameEvent<T> : GameEventBase
    {
        [Space(2)]
        [SerializeField] private Action<T> _response;
        public bool HasListeners => _response != null;

        public virtual void Invoke(T parameter)
        {
            DebugLogger.LogError($"Response is null for {name}", _response == null);

            _response.Invoke(parameter);
        }

        public void AddListener(Action<T> method) => _response += method;
        public void RemoveListener(Action<T> method) => _response -= method;

        public static GameEvent<T> operator +(GameEvent<T> gameEvent, Action<T> method)
        {
            gameEvent.AddListener(method);
            return gameEvent;
        }
        public static GameEvent<T> operator -(GameEvent<T> gameEvent, Action<T> method)
        {
            gameEvent.RemoveListener(method);
            return gameEvent;
        }
    }
}