using System.Diagnostics;
using AiTest.Utils;

namespace AiTest
{
    public static class ObjectPoolHelper
    {
        private static readonly ObjectPool<Stopwatch> _watches = new ObjectPool<Stopwatch>(8);

        public static Stopwatch Get()
        {
            return _watches.Get();
        }

        public static void Return(Stopwatch sw)
        {
            _watches.Put(sw);
        }

    }
}