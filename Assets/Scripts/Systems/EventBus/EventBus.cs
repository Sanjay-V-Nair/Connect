using System;
using System.Collections.Generic;

namespace Connect.Systems.EventBus {
    public interface IEvent { }
    
    public static class EventBus<T> where T : IEvent {
        private static readonly HashSet<Action<T>> _listeners = new HashSet<Action<T>>();

        public static void Subscribe(Action<T> listener) => _listeners.Add(listener);
        public static void Unsubscribe(Action<T> listener) => _listeners.Remove(listener);

        public static void Raise(T eventData) {
            foreach (var listener in _listeners) listener?.Invoke(eventData);
        }
    }

}