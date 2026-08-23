using HarmonyLib;
using UnityEngine;
using Verse;

namespace SpeedRim.Patches
{
    /// <summary>
    /// Drives the per-frame measurement and the framerate governor. This runs on the tick loop
    /// rather than on the GUI so the readings keep coming while the interface is hidden, and so
    /// exactly one sample is taken per frame.
    /// </summary>
    [HarmonyPatch(typeof(TickManager), nameof(TickManager.TickManagerUpdate))]
    public static class TickManager_TickManagerUpdate_Patch
    {
        public static void Postfix(TickManager __instance)
        {
            SpeedRimSpeedMeter.Sample(__instance);

            // Only frames this mod is responsible for may move the governor. A paused game, or a
            // frame that is slow for reasons of its own at vanilla speeds, is not evidence that the
            // extra speeds are too fast.
            if (!__instance.Paused && SpeedRimSpeeds.IsExtraSpeed(__instance.CurTimeSpeed))
            {
                SpeedRimFpsGovernor.Update(SpeedRimSpeedMeter.MeasuredFramesPerSecond);
            }
        }
    }
}
