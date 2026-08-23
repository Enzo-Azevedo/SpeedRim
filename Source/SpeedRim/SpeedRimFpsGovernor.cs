using UnityEngine;
using Verse;

namespace SpeedRim
{
    /// <summary>
    /// Trades game speed for framerate while one of the extra speeds is active.
    /// <para>
    /// Vanilla already refuses to spend a whole frame ticking: its loop gives up once it has burned
    /// a budget derived from <c>TickManager.WorstAllowedFPS</c>. The trouble is that the budget is
    /// fixed and generous - a frame is allowed to spend some 45 ms simulating, which pins the game
    /// at around 22 FPS whenever the CPU cannot keep up. That field is readonly, so this governor
    /// reaches the same result from the other side: it scales down the multiplier this mod asks
    /// for until the measured framerate comes back to the floor the player chose.
    /// </para>
    /// <para>
    /// The controller drops speed quickly and takes it back slowly, which is what keeps it from
    /// oscillating around the target. It never touches vanilla's own speeds.
    /// </para>
    /// </summary>
    public static class SpeedRimFpsGovernor
    {
        public const float MinimumFpsFloor = 15f;
        public const float MinimumFpsCeiling = 120f;

        /// <summary>
        /// Speed is never governed below this multiplier. If the game cannot hold the target
        /// framerate even at vanilla's Fast, the extra speeds are not what is costing the frame.
        /// </summary>
        public const float SlowestGovernedMultiplier = 3f;

        private const float Tolerance = 0.05f;
        private const float DropRate = 0.2f;
        private const float RecoverRate = 0.02f;

        private static float scale = 1f;

        /// <summary>Fraction of the requested speed currently granted, 0-1.</summary>
        public static float Scale
        {
            get { return scale; }
        }

        public static bool IsHoldingBack
        {
            get { return SpeedRimMod.Settings.keepMinimumFps && scale < 0.99f; }
        }

        /// <summary>Applies the current allowance to a requested multiplier.</summary>
        public static float Govern(float requestedMultiplier)
        {
            if (!SpeedRimMod.Settings.keepMinimumFps)
            {
                return requestedMultiplier;
            }

            float governed = requestedMultiplier * scale;
            return Mathf.Clamp(governed, Mathf.Min(SlowestGovernedMultiplier, requestedMultiplier), requestedMultiplier);
        }

        /// <summary>Call once per rendered frame, after the framerate reading has been updated.</summary>
        public static void Update(float measuredFramesPerSecond)
        {
            SpeedRimSettings settings = SpeedRimMod.Settings;
            if (!settings.keepMinimumFps)
            {
                scale = 1f;
                return;
            }

            if (measuredFramesPerSecond <= 0f)
            {
                return;
            }

            float target = Mathf.Clamp(settings.minimumFps, MinimumFpsFloor, MinimumFpsCeiling);
            if (measuredFramesPerSecond < target * (1f - Tolerance))
            {
                scale = Mathf.Max(0f, scale * (1f - DropRate));
            }
            else if (measuredFramesPerSecond > target * (1f + Tolerance))
            {
                scale = Mathf.Min(1f, scale + RecoverRate);
            }
        }

        /// <summary>Forgets the current allowance, so a new game or a new colony starts unthrottled.</summary>
        public static void Reset()
        {
            scale = 1f;
        }
    }
}
