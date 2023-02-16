using UnityEngine;

namespace AriUtomo.Helper
{
    public static class MathExtend
    {
        public static float Remap(this float value, float src_min, float src_max, float dest_min, float dest_max)
        {
            return dest_min + ((dest_max - dest_min) / (src_max - src_min)) * (value - src_min);
        }
    }
}