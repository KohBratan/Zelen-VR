using System.Diagnostics;
using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Мониторинг производительности участков кода.</summary>
    public sealed class PerformanceMonitor
    {
        #region Поля
        private readonly Stopwatch _watch = new Stopwatch();
        private long _ticks;
        private long _samples;
        private float _lastMs;
        #endregion

        #region Замеры
        public void Begin()
        {
            _watch.Reset();
            _watch.Start();
        }

        public float End()
        {
            _watch.Stop();
            _lastMs = (float)_watch.Elapsed.TotalMilliseconds;
            _ticks += _watch.ElapsedTicks;
            _samples++;
            return _lastMs;
        }

        /// <summary>Обернуть действие и вернуть длительность в мс.</summary>
        public float Measure(System.Action action)
        {
            if (action == null) return 0f;
            Begin();
            action();
            return End();
        }
        #endregion

        #region Статистика
        public long Samples { get { return _samples; } }
        public float LastMs { get { return _lastMs; } }

        public float AverageMs
        {
            get
            {
                if (_samples == 0) return 0f;
                double ticks = _ticks;
                return (float)(ticks / (double)_samples / Stopwatch.Frequency * 1000.0);
            }
        }

        public void Reset()
        {
            _ticks = 0;
            _samples = 0;
            _lastMs = 0f;
            _watch.Reset();
        }
        #endregion
    }
}
