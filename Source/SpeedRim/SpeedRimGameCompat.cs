using System;
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
        private static readonly Func<TickManager, AcceptanceReport> PlayerCanControlGetter = ResolvePlayerCanControl();

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

            if (PlayerCanControlGetter == null)
            {
                return true;
            }

            try
            {
                return PlayerCanControlGetter(tickManager).Accepted;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private static Func<TickManager, AcceptanceReport> ResolvePlayerCanControl()
        {
            try
            {
                if (AccessTools.PropertyGetter(typeof(TickManager), "PlayerCanControl") == null)
                {
                    return null;
                }

                return AccessTools.MethodDelegate<Func<TickManager, AcceptanceReport>>(
                    AccessTools.PropertyGetter(typeof(TickManager), "PlayerCanControl"));
            }
            catch (Exception)
            {
                // Not fatal: without it the extra speeds are simply always available.
                return null;
            }
        }
    }
}
