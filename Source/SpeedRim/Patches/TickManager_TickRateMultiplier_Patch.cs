using HarmonyLib;
using Verse;

namespace SpeedRim.Patches
{
    /// <summary>
    /// Supplies the tick rate for the speeds this mod adds. Vanilla's getter only knows
    /// TimeSpeed 0-4 and returns -1 for anything else, so the extra speeds are answered here
    /// and the original getter is skipped; vanilla speeds fall through untouched.
    /// </summary>
    [HarmonyPatch(typeof(TickManager), nameof(TickManager.TickRateMultiplier), MethodType.Getter)]
    public static class TickManager_TickRateMultiplier_Patch
    {
        public static bool Prefix(TickManager __instance, ref float __result)
        {
            TimeSpeed speed = __instance.CurTimeSpeed;
            if (!SpeedRimSpeeds.IsExtraSpeed(speed))
            {
                return true;
            }

            if (__instance.Paused)
            {
                __result = 0f;
                return false;
            }

            // Raids landing, quest events and the like ask the game to drop back to normal speed
            // for a moment. Vanilla obeys that for every speed; so do we, unless told otherwise.
            if (SpeedRimMod.Settings.respectForcedNormalSpeed
                && __instance.slower != null
                && __instance.slower.ForcedNormalSpeed)
            {
                __result = 1f;
                return false;
            }

            float multiplier = SpeedRimSpeeds.MultiplierOf(speed);

            // With no map loaded (caravan travel on the world map) vanilla runs far faster than
            // any on-map speed. Never make that case slower than vanilla would have been.
            if (Current.ProgramState == ProgramState.Playing
                && Current.Game != null
                && Find.Maps.Count == 0
                && multiplier < SpeedRimSpeeds.NoMapsTickRateMultiplier)
            {
                multiplier = SpeedRimSpeeds.NoMapsTickRateMultiplier;
            }

            __result = multiplier;
            return false;
        }
    }
}
