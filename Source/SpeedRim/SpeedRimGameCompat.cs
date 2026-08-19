using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace SpeedRim
{
    /// <summary>
    /// Small reflection helpers for vanilla members that are not public, kept in one place so a
    /// future RimWorld update can only ever degrade behaviour instead of throwing.
    /// </summary>
    internal static class SpeedRimGameCompat
    {
        private static Func<TickManager, AcceptanceReport> playerCanControlGetter = ResolvePlayerCanControl();

        /// <summary>
        /// Whether the player is allowed to change the game speed at all right now. Vanilla uses
        /// this to lock the time controls during scripted moments; the extra speeds obey it too.
        /// </summary>
        public static bool PlayerCanControlTime(TickManager tickManager)
        {
            if (tickManager == null)
            {
                return false;
            }

            Func<TickManager, AcceptanceReport> getter = playerCanControlGetter;
            if (getter == null)
            {
                return true;
            }

            try
            {
                return getter(tickManager).Accepted;
            }
            catch (Exception exception)
            {
                // This runs every frame, so stop asking after the first failure. Dropping the
                // getter also keeps the report below to a single entry instead of a log flood.
                playerCanControlGetter = null;
                Log.Error("[SpeedRim] TickManager.PlayerCanControl threw; the extra speeds will "
                          + "ignore it for the rest of this session: " + exception);
                return true;
            }
        }

        private static Func<TickManager, AcceptanceReport> ResolvePlayerCanControl()
        {
            try
            {
                MethodInfo getter = AccessTools.PropertyGetter(typeof(TickManager), "PlayerCanControl");
                if (getter == null)
                {
                    return null;
                }

                return AccessTools.MethodDelegate<Func<TickManager, AcceptanceReport>>(getter);
            }
            catch (Exception)
            {
                // Not fatal: without it the extra speeds are simply always available.
                return null;
            }
        }
    }
}
