using UnityEngine;
namespace Chud.Backend
{
    public static class RpcRateLimiter
    {
        public const int Capacity = 500;

        private const float WindowSeconds = 1f;

        private static readonly float[] EventTimes = new float[Capacity];
        private static readonly object Gate = new object();

        private static int _head;
        private static int _count;

        public static int RejectedCount { get; private set; }

        public static void Reset()
        {
            lock (Gate)
            {
                _head = 0;
                _count = 0;
                RejectedCount = 0;
            }
        }

        public static bool Prefix()
        {
            lock (Gate)
            {
                float now = Time.unscaledTime;

                while (_count > 0 && now - EventTimes[_head] > WindowSeconds)
                {
                    _head = (_head + 1) % Capacity;
                    _count--;
                }

                if (_count >= Capacity)
                {
                    RejectedCount++;
                    return false;
                }

                EventTimes[(_head + _count) % Capacity] = now;
                _count++;
                return true;
            }
        }
    }
}