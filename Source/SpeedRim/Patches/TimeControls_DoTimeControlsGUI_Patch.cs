using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SpeedRim.Patches
{
    /// <summary>
    /// Adds the extra speed buttons next to vanilla's time controls and handles their key bindings.
    /// The key handling runs as a prefix because vanilla consumes the key events at the end of
    /// DoTimeControlsGUI; the buttons are drawn as a postfix, to the left of the vanilla row.
    /// </summary>
    [HarmonyPatch(typeof(TimeControls), nameof(TimeControls.DoTimeControlsGUI))]
    public static class TimeControls_DoTimeControlsGUI_Patch
    {
        public static void Prefix()
        {
            if (Event.current == null || Event.current.type != EventType.KeyDown)
            {
                return;
            }

            TickManager tickManager = Find.TickManager;
            if (tickManager == null || !SpeedRimGameCompat.PlayerCanControlTime(tickManager))
            {
                return;
            }

            for (int tier = 0; tier < SpeedRimSpeeds.Extra.Length; tier++)
            {
                TimeSpeed speed = SpeedRimSpeeds.Extra[tier];
                KeyBindingDef binding = SpeedRimSpeeds.KeyBindingOf(speed);
                if (binding != null && binding.KeyDownEvent)
                {
                    SpeedRimSpeeds.SetTimeSpeed(speed);
                    Event.current.Use();
                }
            }

            if (!SpeedRimMod.Settings.extendFasterSlowerKeys)
            {
                return;
            }

            // Taken over from vanilla so the ladder can climb past Superfast into 5x/10x/20x.
            if (KeyBindingDefOf.TimeSpeed_Faster.KeyDownEvent)
            {
                SpeedRimSpeeds.StepSpeed(1);
                Event.current.Use();
            }

            if (KeyBindingDefOf.TimeSpeed_Slower.KeyDownEvent)
            {
                SpeedRimSpeeds.StepSpeed(-1);
                Event.current.Use();
            }
        }

        private static readonly string[] CachedTooltips = new string[3];
        private static readonly float[] CachedMultipliers = new float[3] { -1f, -1f, -1f };
        private static readonly KeyCode[] CachedMainKeys = new KeyCode[3] { KeyCode.None, KeyCode.None, KeyCode.None };

        private const float ReadoutWidth = 92f;
        private const float ReadoutGap = 4f;

        public static void Postfix(Rect timerRect)
        {
            if (Event.current == null || Event.current.type == EventType.Layout)
            {
                return;
            }

            TickManager tickManager = Find.TickManager;
            if (tickManager == null)
            {
                return;
            }

            SpeedRimSettings settings = SpeedRimMod.Settings;
            float rowLeft = timerRect.x + settings.buttonOffsetX;
            float rowTop = timerRect.y + settings.buttonOffsetY;
            float rowHeight = TimeControls.TimeButSize.y;

            if (settings.showSpeedButtons)
            {
                float rowWidth = TimeControls.TimeButSize.x * SpeedRimSpeeds.Extra.Length;
                Rect rowRect = new Rect(rowLeft - rowWidth, rowTop, rowWidth, rowHeight);
                DrawSpeedButtons(rowRect, tickManager);
                rowLeft = rowRect.x;
            }

            if (settings.showSpeedReadout)
            {
                // Measuring happens on the tick loop; here we only report it.
                DrawSpeedReadout(new Rect(rowLeft - ReadoutWidth - ReadoutGap, rowTop, ReadoutWidth, rowHeight));
            }
        }

        private static void DrawSpeedButtons(Rect rowRect, TickManager tickManager)
        {
            bool interactive = SpeedRimGameCompat.PlayerCanControlTime(tickManager);
            float buttonWidth = TimeControls.TimeButSize.x;

            GUI.BeginGroup(rowRect);
            Rect buttonRect = new Rect(0f, 0f, buttonWidth, rowRect.height);
            for (int tier = 0; tier < SpeedRimSpeeds.Extra.Length; tier++)
            {
                TimeSpeed speed = SpeedRimSpeeds.Extra[tier];
                if (DrawSpeedButton(buttonRect, tier, speed, interactive) && interactive)
                {
                    SpeedRimSpeeds.SetTimeSpeed(speed);
                }

                if (tickManager.CurTimeSpeed == speed)
                {
                    GUI.DrawTexture(buttonRect, TexUI.HighlightTex);
                }

                buttonRect.x += buttonWidth;
            }

            GUI.EndGroup();

            // Without this a click on our buttons would also reach the map underneath.
            GenUI.AbsorbClicksInRect(rowRect);
            UIHighlighter.HighlightOpportunity(rowRect, "SpeedRimTimeControls");
        }

        /// <summary>Shows the speed the game is reaching, which at high multipliers is not the one asked for.</summary>
        private static void DrawSpeedReadout(Rect readoutRect)
        {
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleRight;

            string label = "SpeedRim.Readout".Translate(
                SpeedRimSpeedMeter.MeasuredMultiplier.ToString("0.0"),
                SpeedRimSpeedMeter.MeasuredFramesPerSecond.ToString("0"));
            Widgets.Label(readoutRect, label);

            Text.Font = font;
            Text.Anchor = anchor;

            string tooltip = SpeedRimFpsGovernor.IsHoldingBack
                ? "SpeedRim.ReadoutTipGoverned".Translate(
                    SpeedRimMod.Settings.minimumFps.ToString("0"),
                    Mathf.RoundToInt(SpeedRimFpsGovernor.Scale * 100f).ToString())
                : "SpeedRim.ReadoutTip".Translate();
            TooltipHandler.TipRegion(readoutRect, tooltip);
        }

        private static bool DrawSpeedButton(Rect buttonRect, int tier, TimeSpeed speed, bool interactive)
        {
            Texture2D texture = tier < SpeedRimTextures.SpeedButtons.Length ? SpeedRimTextures.SpeedButtons[tier] : null;
            Color color = interactive ? Color.white : Color.grey;

            if (texture == null)
            {
                // The texture failed to load: a labelled button still leaves the mod usable.
                return Widgets.ButtonText(buttonRect, SpeedRimSpeeds.LabelOf(speed), true, true, interactive);
            }

            return Widgets.ButtonImage(buttonRect, texture, color, true, TooltipFor(speed));
        }

        private static string TooltipFor(TimeSpeed speed)
        {
            int tier = SpeedRimSpeeds.TierIndexOf(speed);
            float multiplier = SpeedRimSpeeds.MultiplierOf(speed);
            KeyBindingDef binding = SpeedRimSpeeds.KeyBindingOf(speed);
            KeyCode mainKey = binding != null ? binding.MainKey : KeyCode.None;

            if (tier >= 0 && tier < CachedTooltips.Length
                && CachedTooltips[tier] != null
                && CachedMultipliers[tier] == multiplier
                && CachedMainKeys[tier] == mainKey)
            {
                return CachedTooltips[tier];
            }

            string speedLabel = SpeedRimSpeeds.LabelOf(speed);
            string tooltip;
            if (binding == null || mainKey == KeyCode.None)
            {
                tooltip = "SpeedRim.ButtonTooltip".Translate(speedLabel);
            }
            else
            {
                tooltip = "SpeedRim.ButtonTooltipWithKey".Translate(speedLabel, binding.MainKeyLabel);
            }

            if (tier >= 0 && tier < CachedTooltips.Length)
            {
                CachedMultipliers[tier] = multiplier;
                CachedMainKeys[tier] = mainKey;
                CachedTooltips[tier] = tooltip;
            }

            return tooltip;
        }
    }
}
