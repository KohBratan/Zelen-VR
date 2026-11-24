using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Сбор статистики по кадрам.</summary>
    public sealed class FrameStats
    {
        #region Поля
        private float _smoothedDelta = 0.016f;
        private float _minFps = float.MaxValue;
        private float _maxFps;
        private int _frames;
        #endregion

        #region Обновление
        public void Tick()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f) return;
            _smoothedDelta = Mathf.Lerp(_smoothedDelta, dt, 0.1f);
            float fps = 1f / dt;
            if (fps < _minFps) _minFps = fps;
            if (fps > _maxFps) _maxFps = fps;
            _frames++;
        }
        #endregion

        #region Свойства
        public float SmoothedFps { get { return _smoothedDelta > 0f ? 1f / _smoothedDelta : 0f; } }
        public float InstantFps { get { return Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f; } }
        public float MinFps { get { return _minFps == float.MaxValue ? 0f : _minFps; } }
        public float MaxFps { get { return _maxFps; } }
        public int Frames { get { return _frames; } }
        #endregion

        #region Сброс
        public void Reset()
        {
            _smoothedDelta = 0.016f;
            _minFps = float.MaxValue;
            _maxFps = 0f;
            _frames = 0;
        }
        #endregion
    }
}
