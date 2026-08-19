using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SpeedRim
{
    /// <summary>
    /// The extra time speeds this mod adds on top of vanilla's <see cref="TimeSpeed"/> range (0-4).
    /// The active speed is stored in vanilla's own <c>TickManager.curTimeSpeed</c>, so pausing,
    /// unpausing and other mods that change the speed keep working without extra bookkeeping.
    /// Saves are sanitised back to a vanilla value (see TickManager_ExposeData_Patch) so a save
    /// made with this mod still loads correctly after the mod is removed.
    /// </summary>
    public static class SpeedRimSpeeds
    {
        public const TimeSpeed Speed5x = (TimeSpeed)5;
        public const TimeSpeed Speed10x = (TimeSpeed)6;
        public const TimeSpeed Speed20x = (TimeSpeed)7;

        /// <summary>Speeds added by this mod, slowest first.</summary>
        public static readonly TimeSpeed[] Extra = { Speed5x, Speed10x, Speed20x };

        /// <summary>Order used by the "faster"/"slower" key bindings, slowest first.</summary>
        public static readonly TimeSpeed[] Ladder =
        {
            TimeSpeed.Normal,
            TimeSpeed.Fast,
            TimeSpeed.Superfast,
            Speed5x,
            Speed10x,
            Speed20x
        };

        /// <summary>Vanilla speeds up world-map-only play (no maps loaded) far past the normal rates.</summary>
        public const float NoMapsTickRateMultiplier = 150f;

        public const float MinMultiplier = 2f;
        public const float MaxMultiplier = 100f;

        public static bool IsExtraSpeed(TimeSpeed speed)
        {
            return speed >= Speed5x && speed <= Speed20x;
        }

        /// <summary>0, 1 or 2 for the three extra speeds; -1 for any vanilla speed.</summary>
        public static int TierIndexOf(TimeSpeed speed)
        {
            return IsExtraSpeed(speed) ? (int)speed - (int)Speed5x : -1;
        }

        /// <summary>Tick rate multiplier configured for one of the extra speeds.</summary>
        public static float MultiplierOf(TimeSpeed speed)
        {
            SpeedRimSettings settings = SpeedRimMod.Settings;
            switch (speed)
            {
                case Speed5x:
                    return settings.multiplierTier1;
                case Speed10x:
                    return settings.multiplierTier2;
                case Speed20x:
                    return settings.multiplierTier3;
                default:
                    return 1f;
            }
        }

        /// <summary>Short label such as "10x", used for tooltips and settings.</summary>
        public static string LabelOf(TimeSpeed speed)
        {
            return MultiplierOf(speed).ToString("0.##") + "x";
        }

        public static KeyBindingDef KeyBindingOf(TimeSpeed speed)
        {
            switch (speed)
            {
                case Speed5x:
                    return SpeedRimKeyBindingDefOf.SpeedRim_Speed5x;
                case Speed10x:
                    return SpeedRimKeyBindingDefOf.SpeedRim_Speed10x;
                case Speed20x:
                    return SpeedRimKeyBindingDefOf.SpeedRim_Speed20x;
                default:
                    return null;
            }
        }

        /// <summary>Applies a speed the same way vanilla's own time buttons do.</summary>
        public static void SetTimeSpeed(TimeSpeed speed)
        {
            TickManager tickManager = Find.TickManager;
            if (tickManager == null)
            {
                return;
            }

            if (speed == TimeSpeed.Paused)
            {
                tickManager.TogglePaused();
            }
            else
            {
                tickManager.CurTimeSpeed = speed;
            }

            PlaySoundOf(tickManager.CurTimeSpeed);
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(
                speed == TimeSpeed.Paused ? ConceptDefOf.Pause : ConceptDefOf.TimeControls,
                KnowledgeAmount.SpecificInteraction);
        }

        /// <summary>Steps one rung up (+1) or down (-1) the speed ladder, including the extra speeds.</summary>
        public static void StepSpeed(int direction)
        {
            TickManager tickManager = Find.TickManager;
            if (tickManager == null)
            {
                return;
            }

            TimeSpeed current = tickManager.CurTimeSpeed;
            int index;
            if (current == TimeSpeed.Paused)
            {
                // Paused sits one rung below Normal.
                index = -1;
            }
            else if (current == TimeSpeed.Ultrafast)
            {
                // Dev-mode-only vanilla speed: it sits between Superfast and the first extra speed,
                // so stepping up lands on 5x and stepping down lands on Superfast.
                int superfast = Array.IndexOf(Ladder, TimeSpeed.Superfast);
                index = direction > 0 ? superfast : superfast + 1;
            }
            else
            {
                index = Array.IndexOf(Ladder, current);
                if (index < 0)
                {
                    index = 0;
                }
            }

            int next = Mathf.Clamp(index + direction, -1, Ladder.Length - 1);
            if (next == index)
            {
                return;
            }

            SetTimeSpeed(next < 0 ? TimeSpeed.Paused : Ladder[next]);
        }

        private static void PlaySoundOf(TimeSpeed speed)
        {
            SoundDef soundDef;
            switch (speed)
            {
                case TimeSpeed.Paused:
                    soundDef = SoundDefOf.Clock_Stop;
                    break;
                case TimeSpeed.Normal:
                    soundDef = SoundDefOf.Clock_Normal;
                    break;
                case TimeSpeed.Fast:
                    soundDef = SoundDefOf.Clock_Fast;
                    break;
                default:
                    soundDef = SoundDefOf.Clock_Superfast;
                    break;
            }

            if (soundDef != null)
            {
                soundDef.PlayOneShotOnCamera();
            }
        }
    }
}
