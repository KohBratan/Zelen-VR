using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Таймер перезарядки способностей.</summary>
    public sealed class CooldownTimer
    {
        #region Поля
        private float _cooldown;
        private float _readyAt;
        private bool _active;
        #endregion

        #region Управление
        public void Trigger(float cooldown)
        {
            _cooldown = Mathf.Max(0f, cooldown);
            _readyAt = Time.time + _cooldown;
            _active = _cooldown > 0f;
        }

        public void Clear()
        {
            _active = false;
            _readyAt = 0f;
        }
        #endregion

        #region Проверки
        public bool IsReady()
        {
            return !_active || Time.time >= _readyAt;
        }

        public float Remaining()
        {
            if (!_active) return 0f;
            return Mathf.Max(0f, _readyAt - Time.time);
        }

        public float Progress()
        {
            if (!_active || _cooldown <= 0f) return 1f;
            return Mathf.Clamp01(1f - Remaining() / _cooldown);
        }
        #endregion

        #region Свойства
        public float Cooldown { get { return _cooldown; } }
        #endregion
    }
}
