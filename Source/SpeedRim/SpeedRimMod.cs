using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace SpeedRim
{
    /// <summary>Mod entry point: applies the Harmony patches and draws the settings page.</summary>
    public class SpeedRimMod : Mod
    {
        public const string PackageId = "enzoazevedo.speedrim";

        private static SpeedRimSettings settings;

        public SpeedRimMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<SpeedRimSettings>();
            settings.Clamp();

            Harmony harmony = new Harmony(PackageId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static SpeedRimSettings Settings
        {
            // Never null: RimWorld constructs the Mod before anything can ask for a tick rate,
            // but a fallback keeps the patches harmless if that ever changes.
            get { return settings ?? (settings = new SpeedRimSettings()); }
        }

        public override string SettingsCategory()
        {
            return "SpeedRim.ModTitle".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            SpeedRimSettings s = Settings;

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("SpeedRim.Settings.MultipliersHeader".Translate());
            listing.Gap(4f);

            for (int tier = 0; tier < SpeedRimSpeeds.Extra.Length; tier++)
            {
                float current = s.MultiplierForTier(tier);
                string label = "SpeedRim.Settings.MultiplierLabel".Translate(tier + 1, current.ToString("0.##"));
                float updated = listing.SliderLabeled(label, current, SpeedRimSpeeds.MinMultiplier, SpeedRimSpeeds.MaxMultiplier);
                s.SetMultiplierForTier(tier, Mathf.Round(updated));
            }

            listing.Gap(6f);
            listing.Label("SpeedRim.Settings.MultipliersNote".Translate());
            listing.GapLine();

            listing.CheckboxLabeled("SpeedRim.Settings.ShowButtons".Translate(), ref s.showSpeedButtons,
                "SpeedRim.Settings.ShowButtonsTip".Translate());
            listing.CheckboxLabeled("SpeedRim.Settings.ExtendKeys".Translate(), ref s.extendFasterSlowerKeys,
                "SpeedRim.Settings.ExtendKeysTip".Translate());
            listing.CheckboxLabeled("SpeedRim.Settings.RespectForcedNormal".Translate(), ref s.respectForcedNormalSpeed,
                "SpeedRim.Settings.RespectForcedNormalTip".Translate());

            listing.GapLine();
            listing.Label("SpeedRim.Settings.ButtonOffsetHeader".Translate());
            s.buttonOffsetX = Mathf.Round(listing.SliderLabeled(
                "SpeedRim.Settings.ButtonOffsetX".Translate(s.buttonOffsetX.ToString("0")), s.buttonOffsetX, -400f, 400f));
            s.buttonOffsetY = Mathf.Round(listing.SliderLabeled(
                "SpeedRim.Settings.ButtonOffsetY".Translate(s.buttonOffsetY.ToString("0")), s.buttonOffsetY, -400f, 400f));

            listing.Gap(8f);
            if (listing.ButtonText("SpeedRim.Settings.Reset".Translate()))
            {
                s.ResetToDefaults();
            }

            listing.End();
        }

        public override void WriteSettings()
        {
            Settings.Clamp();
            base.WriteSettings();
        }
    }
}
