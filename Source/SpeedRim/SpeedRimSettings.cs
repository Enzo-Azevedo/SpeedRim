using UnityEngine;
using Verse;

namespace SpeedRim
{
    /// <summary>Player-configurable options, stored in RimWorld's ModsConfig folder.</summary>
    public class SpeedRimSettings : ModSettings
    {
        public const float DefaultMultiplierTier1 = 5f;
        public const float DefaultMultiplierTier2 = 10f;
        public const float DefaultMultiplierTier3 = 20f;

        public float multiplierTier1 = DefaultMultiplierTier1;
        public float multiplierTier2 = DefaultMultiplierTier2;
        public float multiplierTier3 = DefaultMultiplierTier3;

        /// <summary>Draw the three extra buttons next to the vanilla speed buttons.</summary>
        public bool showSpeedButtons = true;

        /// <summary>Let the vanilla "faster"/"slower" keys step into the extra speeds.</summary>
        public bool extendFasterSlowerKeys = true;

        /// <summary>Honour vanilla's temporary forced-normal-speed (raid arriving, etc.).</summary>
        public bool respectForcedNormalSpeed = true;

        /// <summary>Fine tuning for the button row position, in pixels.</summary>
        public float buttonOffsetX;
        public float buttonOffsetY;

        public float MultiplierForTier(int tier)
        {
            switch (tier)
            {
                case 0: return multiplierTier1;
                case 1: return multiplierTier2;
                case 2: return multiplierTier3;
                default: return 1f;
            }
        }

        public void SetMultiplierForTier(int tier, float value)
        {
            value = Mathf.Clamp(value, SpeedRimSpeeds.MinMultiplier, SpeedRimSpeeds.MaxMultiplier);
            switch (tier)
            {
                case 0:
                    multiplierTier1 = value;
                    break;
                case 1:
                    multiplierTier2 = value;
                    break;
                case 2:
                    multiplierTier3 = value;
                    break;
            }
        }

        public void ResetToDefaults()
        {
            multiplierTier1 = DefaultMultiplierTier1;
            multiplierTier2 = DefaultMultiplierTier2;
            multiplierTier3 = DefaultMultiplierTier3;
            showSpeedButtons = true;
            extendFasterSlowerKeys = true;
            respectForcedNormalSpeed = true;
            buttonOffsetX = 0f;
            buttonOffsetY = 0f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref multiplierTier1, "multiplierTier1", DefaultMultiplierTier1);
            Scribe_Values.Look(ref multiplierTier2, "multiplierTier2", DefaultMultiplierTier2);
            Scribe_Values.Look(ref multiplierTier3, "multiplierTier3", DefaultMultiplierTier3);
            Scribe_Values.Look(ref showSpeedButtons, "showSpeedButtons", true);
            Scribe_Values.Look(ref extendFasterSlowerKeys, "extendFasterSlowerKeys", true);
            Scribe_Values.Look(ref respectForcedNormalSpeed, "respectForcedNormalSpeed", true);
            Scribe_Values.Look(ref buttonOffsetX, "buttonOffsetX");
            Scribe_Values.Look(ref buttonOffsetY, "buttonOffsetY");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Clamp();
            }
        }

        /// <summary>Keeps hand-edited config files from producing a frozen or negative tick rate.</summary>
        public void Clamp()
        {
            multiplierTier1 = Mathf.Clamp(multiplierTier1, SpeedRimSpeeds.MinMultiplier, SpeedRimSpeeds.MaxMultiplier);
            multiplierTier2 = Mathf.Clamp(multiplierTier2, SpeedRimSpeeds.MinMultiplier, SpeedRimSpeeds.MaxMultiplier);
            multiplierTier3 = Mathf.Clamp(multiplierTier3, SpeedRimSpeeds.MinMultiplier, SpeedRimSpeeds.MaxMultiplier);
            buttonOffsetX = Mathf.Clamp(buttonOffsetX, -400f, 400f);
            buttonOffsetY = Mathf.Clamp(buttonOffsetY, -400f, 400f);
        }
    }
}
