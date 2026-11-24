using System;
using System.Collections.Generic;

namespace ZelenVR.Utilities
{
    /// <summary>Простой конечный автомат состояний.</summary>
    public sealed class StateMachine<TState> where TState : struct, IComparable
    {
        #region Поля
        private readonly Dictionary<TState, Action> _onEnter = new Dictionary<TState, Action>();
        private readonly Dictionary<TState, Action> _onExit = new Dictionary<TState, Action>();
        private readonly Dictionary<TState, Action> _onUpdate = new Dictionary<TState, Action>();
        private TState _current;
        private bool _hasState;
        #endregion

        #region Регистрация
        public void Register(TState state, Action onEnter = null, Action onUpdate = null, Action onExit = null)
        {
            if (onEnter != null) _onEnter[state] = onEnter;
            if (onUpdate != null) _onUpdate[state] = onUpdate;
            if (onExit != null) _onExit[state] = onExit;
        }
        #endregion

        #region Переходы
        public void Change(TState next)
        {
            if (_hasState && _current.Equals(next)) return;
            if (_hasState && _onExit.TryGetValue(_current, out Action exit)) exit?.Invoke();
            _current = next;
            _hasState = true;
            if (_onEnter.TryGetValue(next, out Action enter)) enter?.Invoke();
        }

        public void Update()
        {
            if (!_hasState) return;
            if (_onUpdate.TryGetValue(_current, out Action upd)) upd?.Invoke();
        }
        #endregion

        #region Свойства
        public TState Current { get { return _current; } }
        public bool HasState { get { return _hasState; } }
        public bool Is(TState state) { return _hasState && _current.Equals(state); }
        #endregion
    }
}
