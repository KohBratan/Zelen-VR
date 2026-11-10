using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Простой таймер обратного отсчёта.</summary>
    public sealed class SimpleTimer
    {
        #region Поля
        private float _duration;
        private float _elapsed;
        private bool _running;
        #endregion

        #region Управление
        public void Start(float duration)
        {
            _duration = Mathf.Max(0f, duration);
            _elapsed = 0f;
            _running = _duration > 0f;
        }

        public void Stop() { _running = false; }
        public void Reset() { _elapsed = 0f; _running = false; }
        #endregion

        #region Обновление
        public bool Tick(float dt)
        {
            if (!_running) return false;
            _elapsed += dt;
            if (_elapsed >= _duration)
            {
                _running = false;
                return true;
            }
            return false;
        }
        #endregion

        #region Свойства
        public bool IsRunning { get { return _running; } }
        public float Remaining { get { return Mathf.Max(0f, _duration - _elapsed); } }
        public float Progress { get { return _duration > 0f ? Mathf.Clamp01(_elapsed / _duration) : 1f; } }
        #endregion
    }
}
