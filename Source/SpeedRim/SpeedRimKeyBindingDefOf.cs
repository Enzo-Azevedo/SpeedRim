using RimWorld;
using Verse;

namespace SpeedRim
{
    [DefOf]
    public static class SpeedRimKeyBindingDefOf
    {
        public static KeyBindingDef SpeedRim_Speed5x;
        public static KeyBindingDef SpeedRim_Speed10x;
        public static KeyBindingDef SpeedRim_Speed20x;

        static SpeedRimKeyBindingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SpeedRimKeyBindingDefOf));
        }
    }
}
