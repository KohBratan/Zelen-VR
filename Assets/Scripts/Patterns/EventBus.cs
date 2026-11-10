using System;
using System.Collections.Generic;

namespace ZelenVR.Utilities
{
    /// <summary>Шина событий с типизированными подписчиками.</summary>
    public sealed class EventBus
    {
        #region Поля
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        #endregion

        #region Подписка
        public void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            Type key = typeof(T);
            if (!_handlers.TryGetValue(key, out List<Delegate> list))
            {
                list = new List<Delegate>();
                _handlers[key] = list;
            }
            if (!list.Contains(handler)) list.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            if (_handlers.TryGetValue(typeof(T), out List<Delegate> list))
            {
                list.Remove(handler);
            }
        }
        #endregion

        #region Публикация
        public void Publish<T>(T payload)
        {
            if (!_handlers.TryGetValue(typeof(T), out List<Delegate> list)) return;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] is Action<T> action) action.Invoke(payload);
            }
        }
        #endregion

        #region Очистка
        public void Clear()
        {
            _handlers.Clear();
        }

        public void Clear<T>()
        {
            _handlers.Remove(typeof(T));
        }
        #endregion
    }
}
