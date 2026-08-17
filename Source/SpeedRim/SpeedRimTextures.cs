using UnityEngine;
using Verse;

namespace SpeedRim
{
    /// <summary>Button textures, loaded once RimWorld has finished loading mod content.</summary>
    [StaticConstructorOnStartup]
    public static class SpeedRimTextures
    {
        public static readonly Texture2D[] SpeedButtons =
        {
            ContentFinder<Texture2D>.Get("SpeedRim/SpeedButton_Tier1"),
            ContentFinder<Texture2D>.Get("SpeedRim/SpeedButton_Tier2"),
            ContentFinder<Texture2D>.Get("SpeedRim/SpeedButton_Tier3")
        };
    }
}
