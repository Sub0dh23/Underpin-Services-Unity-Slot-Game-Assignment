using UnityEngine;

namespace Underpin.SlotGame.Utils
{
    /// <summary>
    /// Pure mathematical easing functions for natural, bouncy, and juice-filled reel animations.
    /// Eliminates external tween library dependencies for maximum performance and compatibility.
    /// </summary>
    public static class EasingHelper
    {
        public static float Linear(float t) => t;

        public static float EaseInQuad(float t) => t * t;

        public static float EaseOutQuad(float t) => t * (2f - t);

        public static float EaseInOutQuad(float t) => t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;

        public static float EaseInCubic(float t) => t * t * t;

        public static float EaseOutCubic(float t)
        {
            float f = t - 1f;
            return f * f * f + 1f;
        }

        public static float EaseInOutCubic(float t)
        {
            return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;
        }

        /// <summary>
        /// Anticipation easing: pulls back slightly before accelerating forward.
        /// </summary>
        public static float EaseInBack(float t, float overshoot = 1.70158f)
        {
            return t * t * ((overshoot + 1f) * t - overshoot);
        }

        /// <summary>
        /// Overshoot landing: moves past target slightly and bounces back into place.
        /// </summary>
        public static float EaseOutBack(float t, float overshoot = 1.70158f)
        {
            float f = t - 1f;
            return f * f * ((overshoot + 1f) * f + overshoot) + 1f;
        }

        /// <summary>
        /// Natural bounce on landing.
        /// </summary>
        public static float EaseOutBounce(float t)
        {
            if (t < (1f / 2.75f))
            {
                return 7.5625f * t * t;
            }
            else if (t < (2f / 2.75f))
            {
                t -= (1.5f / 2.75f);
                return 7.5625f * t * t + 0.75f;
            }
            else if (t < (2.5f / 2.75f))
            {
                t -= (2.25f / 2.75f);
                return 7.5625f * t * t + 0.9375f;
            }
            else
            {
                t -= (2.625f / 2.75f);
                return 7.5625f * t * t + 0.984375f;
            }
        }
    }
}
