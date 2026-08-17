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

        public static void Postfix(Rect timerRect)
        {
            if (!SpeedRimMod.Settings.showSpeedButtons)
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
                timerRect.x - buttonWidth * count + SpeedRimMod.Settings.buttonOffsetX,
                timerRect.y + SpeedRimMod.Settings.buttonOffsetY,
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
            KeyBindingDef binding = SpeedRimSpeeds.KeyBindingOf(speed);
            string speedLabel = SpeedRimSpeeds.LabelOf(speed);
            if (binding == null || binding.MainKey == KeyCode.None)
            {
                return "SpeedRim.ButtonTooltip".Translate(speedLabel);
            }

            return "SpeedRim.ButtonTooltipWithKey".Translate(speedLabel, binding.MainKeyLabel);
        }
    }
}
