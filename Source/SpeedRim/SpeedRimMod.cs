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

        /// <summary>Arbitrary but stable key so RimWorld reports the missing settings only once.</summary>
        private const int MissingSettingsLogKey = 0x5DE0;

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
            get
            {
                if (settings == null)
                {
                    // RimWorld constructs the Mod before anything can ask for a tick rate, so this
                    // is a broken assumption rather than a normal path. Falling back to defaults
                    // keeps the game playable, but it should not do so quietly.
                    Log.ErrorOnce(
                        "[SpeedRim] Settings were requested before the mod was constructed. Falling "
                        + "back to the default multipliers; the configured ones are not in effect.",
                        MissingSettingsLogKey);
                    settings = new SpeedRimSettings();
                }

                return settings;
            }
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
            listing.Label("SpeedRim.Settings.FramerateHeader".Translate());
            listing.Label("SpeedRim.Settings.FramerateNote".Translate());
            listing.Gap(4f);
            listing.CheckboxLabeled("SpeedRim.Settings.KeepMinimumFps".Translate(), ref s.keepMinimumFps,
                "SpeedRim.Settings.KeepMinimumFpsTip".Translate());

            if (s.keepMinimumFps)
            {
                s.minimumFps = Mathf.Round(listing.SliderLabeled(
                    "SpeedRim.Settings.MinimumFps".Translate(s.minimumFps.ToString("0")),
                    s.minimumFps,
                    SpeedRimFpsGovernor.MinimumFpsFloor,
                    SpeedRimFpsGovernor.MinimumFpsCeiling));
            }

            listing.CheckboxLabeled("SpeedRim.Settings.ShowReadout".Translate(), ref s.showSpeedReadout,
                "SpeedRim.Settings.ShowReadoutTip".Translate());

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
            SpeedRimFpsGovernor.Reset();
            base.WriteSettings();
        }
    }
}
