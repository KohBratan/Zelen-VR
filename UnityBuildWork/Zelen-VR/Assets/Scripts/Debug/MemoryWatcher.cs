using UnityEngine;

namespace ZelenVR.Utilities
{
    /// <summary>Наблюдение за использованием памяти.</summary>
    public sealed class MemoryWatcher
    {
        #region Поля
        private long _peakBytes;
        private long _lastBytes;
        private float _sampleInterval = 1f;
        private float _timer;
        #endregion

        #region Сэмплирование
        public void Tick(float dt)
        {
            _timer += dt;
            if (_timer < _sampleInterval) return;
            _timer = 0f;
            _lastBytes = CurrentBytes();
            if (_lastBytes > _peakBytes) _peakBytes = _lastBytes;
        }

        public long CurrentBytes()
        {
            return System.GC.GetTotalMemory(false);
        }
        #endregion

        #region Свойства
        public long LastBytes { get { return _lastBytes; } }
        public long PeakBytes { get { return _peakBytes; } }
        public float LastMb { get { return _lastBytes / (1024f * 1024f); } }
        public float PeakMb { get { return _peakBytes / (1024f * 1024f); } }
        #endregion

        #region Управление
        public void SetSampleInterval(float seconds)
        {
            _sampleInterval = Mathf.Max(0.1f, seconds);
        }

        /// <summary>Принудительно собрать мусор.</summary>
        public void Collect()
        {
            System.GC.Collect();
            _lastBytes = CurrentBytes();
        }

        public void Reset()
        {
            _peakBytes = 0;
            _lastBytes = 0;
            _timer = 0f;
        }
        #endregion
    }
}
