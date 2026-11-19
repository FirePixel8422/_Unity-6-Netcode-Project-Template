using System;

namespace Fire_Pixel.Utility
{
    /// <summary>
    /// Container that stores an <see cref="Action"/> which can be subscribed to and is invoked only once.
    /// If subscribed to after invoke already happened, call the subscriber instantly.
    /// </summary>
    public class OneTimeAction
    {
        private event Action InternalAction;
        public bool HasExecuted { get; private set; }


        public void Invoke()
        {
            InternalAction?.Invoke();
            InternalAction = null;
            HasExecuted = true;
        }
        public static OneTimeAction operator +(OneTimeAction e, Action action)
        {
            if (!e.HasExecuted)
            {
                e.InternalAction += action;
            }
            else
            {
                action?.Invoke();
            }
            return e;
        }
        public static OneTimeAction operator -(OneTimeAction e, Action action)
        {
            if (!e.HasExecuted)
            {
                e.InternalAction -= action;
            }
            return e;
        }
    }

    /// <summary>
    /// Container that stores an <see cref="Action{T}"/> which can be subscribed to and is invoked only once.
    /// If subscribed to after invoke already happened, call the subscriber instantly.
    /// </summary>
    public class OneTimeAction<T>
    {
        private event Action<T> InternalAction;
        private T invokedValue;
        public bool HasExecuted { get; private set; }


        public void Invoke(T value)
        {
            if (HasExecuted) return;

            InternalAction?.Invoke(value);
            InternalAction = null;
            HasExecuted = true;
            invokedValue = value;
        }
        public static OneTimeAction<T> operator +(OneTimeAction<T> e, Action<T> action)
        {
            if (!e.HasExecuted)
            {
                e.InternalAction += action;
            }
            else
            {
                action?.Invoke(e.invokedValue);
            }
            return e;
        }
        public static OneTimeAction<T> operator -(OneTimeAction<T> e, Action<T> action)
        {
            if (!e.HasExecuted)
            {
                e.InternalAction -= action;
            }
            return e;
        }
    }
}