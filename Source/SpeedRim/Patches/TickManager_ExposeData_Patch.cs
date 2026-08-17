using System;
using HarmonyLib;
using Verse;

namespace SpeedRim.Patches
{
    /// <summary>
    /// Writes a vanilla time speed into the save file whenever one of this mod's extra speeds is
    /// active, then puts the extra speed straight back. Without this, a save made at 20x would
    /// load with an unknown TimeSpeed value once the mod is uninstalled, leaving the game stuck.
    /// </summary>
    [HarmonyPatch(typeof(TickManager), nameof(TickManager.ExposeData))]
    public static class TickManager_ExposeData_Patch
    {
        /// <summary>The vanilla speed written to the save in place of an extra speed.</summary>
        private const TimeSpeed SaveFallbackSpeed = TimeSpeed.Superfast;

        private static readonly AccessTools.FieldRef<TickManager, TimeSpeed> CurTimeSpeedField = ResolveCurTimeSpeedField();

        public struct SavedSpeeds
        {
            public bool restoreNeeded;
            public TimeSpeed curTimeSpeed;
            public TimeSpeed prePauseTimeSpeed;
        }

        public static void Prefix(TickManager __instance, out SavedSpeeds __state)
        {
            __state = default(SavedSpeeds);
            if (Scribe.mode != LoadSaveMode.Saving || CurTimeSpeedField == null)
            {
                return;
            }

            TimeSpeed current = CurTimeSpeedField(__instance);
            TimeSpeed prePause = __instance.prePauseTimeSpeed;
            if (!SpeedRimSpeeds.IsExtraSpeed(current) && !SpeedRimSpeeds.IsExtraSpeed(prePause))
            {
                return;
            }

            __state.restoreNeeded = true;
            __state.curTimeSpeed = current;
            __state.prePauseTimeSpeed = prePause;

            if (SpeedRimSpeeds.IsExtraSpeed(current))
            {
                CurTimeSpeedField(__instance) = SaveFallbackSpeed;
            }

            if (SpeedRimSpeeds.IsExtraSpeed(prePause))
            {
                __instance.prePauseTimeSpeed = SaveFallbackSpeed;
            }
        }

        public static void Postfix(TickManager __instance, SavedSpeeds __state)
        {
            if (!__state.restoreNeeded || CurTimeSpeedField == null)
            {
                return;
            }

            CurTimeSpeedField(__instance) = __state.curTimeSpeed;
            __instance.prePauseTimeSpeed = __state.prePauseTimeSpeed;
        }

        private static AccessTools.FieldRef<TickManager, TimeSpeed> ResolveCurTimeSpeedField()
        {
            try
            {
                return AccessTools.FieldRefAccess<TickManager, TimeSpeed>("curTimeSpeed");
            }
            catch (Exception exception)
            {
                Log.Warning("[SpeedRim] Could not access TickManager.curTimeSpeed, saves will not be "
                            + "sanitised for playing without this mod: " + exception.Message);
                return null;
            }
        }
    }
}
