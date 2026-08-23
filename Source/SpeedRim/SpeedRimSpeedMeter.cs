using UnityEngine;
using Verse;

namespace SpeedRim
{
    /// <summary>
    /// Measures the speed the game is actually reaching, as opposed to the one that was asked for.
    /// <para>
    /// RimWorld reports how many ticks it managed in the current frame, and 60 ticks per second is
    /// what the game calls 1x, so ticks per second divided by 60 is the multiplier the player is
    /// really getting. Both readings are smoothed: a per-frame number is unreadable.
    /// </para>
    /// </summary>
    public static class SpeedRimSpeedMeter
    {
        public const float TicksPerSecondAtNormalSpeed = 60f;

        /// <summary>Weight of the newest frame. About a one second window at 60 FPS.</summary>
        private const float SmoothingWeight = 0.05f;

        private static float ticksPerSecond;
        private static float framesPerSecond;

        public static float MeasuredMultiplier
        {
            get { return ticksPerSecond / TicksPerSecondAtNormalSpeed; }
        }

        public static float MeasuredFramesPerSecond
        {
            get { return framesPerSecond; }
        }

        /// <summary>Call once per rendered frame, never on a non-repaint GUI event.</summary>
        public static void Sample(TickManager tickManager)
        {
            float deltaTime = Time.deltaTime;
            if (tickManager == null || deltaTime <= 0f)
            {
                return;
            }

            ticksPerSecond = Mathf.Lerp(ticksPerSecond, tickManager.TicksThisFrame / deltaTime, SmoothingWeight);
            framesPerSecond = Mathf.Lerp(framesPerSecond, 1f / deltaTime, SmoothingWeight);
        }
    }
}
