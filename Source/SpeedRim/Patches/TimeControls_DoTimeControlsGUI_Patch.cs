using System;
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

        // Tooltip text only changes when the tier's multiplier, its key binding or the active
        // language changes, so it is built once per change instead of once per frame.
        private static readonly string[] CachedTooltips = new string[SpeedRimSpeeds.Extra.Length];
        private static readonly float[] CachedMultipliers = new float[SpeedRimSpeeds.Extra.Length];
        private static readonly KeyCode[] CachedMainKeys = new KeyCode[SpeedRimSpeeds.Extra.Length];
        private static LoadedLanguage cachedLanguage;

        public static void Postfix(Rect timerRect)
        {
            SpeedRimSettings settings = SpeedRimMod.Settings;
            if (!settings.showSpeedButtons)
            {
                return;
            }

            if (Event.current == null || Event.current.type == EventType.Layout)
            {
                return;
            }

            TickManager tickManager = Find.TickManager;
            if (tickManager == null)
            {
                return;
            }

            bool interactive = SpeedRimGameCompat.PlayerCanControlTime(tickManager);
            float buttonWidth = TimeControls.TimeButSize.x;
            float buttonHeight = TimeControls.TimeButSize.y;
            int count = SpeedRimSpeeds.Extra.Length;

            Rect rowRect = new Rect(
                timerRect.x - buttonWidth * count + settings.buttonOffsetX,
                timerRect.y + settings.buttonOffsetY,
                buttonWidth * count,
                buttonHeight);

            GUI.BeginGroup(rowRect);
            Rect buttonRect = new Rect(0f, 0f, buttonWidth, buttonHeight);
            for (int tier = 0; tier < count; tier++)
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

            bool cacheable = tier >= 0 && tier < CachedTooltips.Length;
            if (cacheable)
            {
                // The language can only be changed from the main menu, but these statics outlive
                // that, so a cached tooltip would otherwise stay in the previous language.
                if (cachedLanguage != LanguageDatabase.activeLanguage)
                {
                    cachedLanguage = LanguageDatabase.activeLanguage;
                    Array.Clear(CachedTooltips, 0, CachedTooltips.Length);
                }

                if (CachedTooltips[tier] != null
                    && CachedMultipliers[tier] == multiplier
                    && CachedMainKeys[tier] == mainKey)
                {
                    return CachedTooltips[tier];
                }
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

            if (cacheable)
            {
                CachedMultipliers[tier] = multiplier;
                CachedMainKeys[tier] = mainKey;
                CachedTooltips[tier] = tooltip;
            }

            return tooltip;
        }
    }
}
