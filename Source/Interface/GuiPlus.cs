using System.Globalization;
using PawnRules.Data;
using PawnRules.Patch;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PawnRules.Interface
{
    [StaticConstructorOnStartup]
    internal static class GuiPlus
    {
        private const float CheckboxSize = 24f;
        private const float ButtonSize = 30f;
        private const float RadioButtonSize = 24f;

        private static readonly Color InactiveColor = new Color(0.37f, 0.37f, 0.37f, 0.8f);
        public static readonly Color ReadOnlyColor = new Color(0.75f, 0.75f, 0.75f, 0.75f);

        private static readonly Texture2D EditRulesTexture = ContentFinder<Texture2D>.Get("PawnRules/EditRules");

        public static Command_Action EditRulesCommand(Pawn pawn) => new Command_Action
                                                                    {
                                                                                icon = EditRulesTexture,
                                                                                defaultLabel = Lang.Get("Gizmo.EditRulesLabel"),
                                                                                defaultDesc = Lang.Get("Gizmo.EditRulesDesc", pawn.GetTargetType()?.Label.ToLower() ?? "pawn"),
                                                                                action = () => Dialog_Rules.OpenFromPawn(pawn)
                                                                    };

        public static bool ButtonText(Rect rect, string label, string tooltip = null, bool enabled = true)
        {
            var result = Widgets.ButtonText(rect, label, true, false, enabled);

            if (Mouse.IsOver(rect) && enabled)
            {
                Widgets.DrawHighlight(rect);
                if (!tooltip.NullOrEmpty()) { TooltipHandler.TipRegion(rect, tooltip); }
            }

            if (!enabled) { Widgets.DrawBoxSolid(rect.ContractedBy(1f), InactiveColor); }
            return result;
        }

        public static bool RadioButtonInverted(Rect rect, string labelText, bool chosen, bool enabled)
        {
            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            var color = GUI.color;
            if (!enabled) { GUI.color = InactiveColor; }
            Widgets.Label(rect.AdjustedBy(RadioButtonSize + 4f, 0f, -(RadioButtonSize + 4f), 0f), labelText);
            GUI.color = color;
            Text.Anchor = anchor;
            var labelClicked = Widgets.ButtonInvisible(rect);
            var radioClicked = Widgets.RadioButton(rect.x, (rect.y + (rect.height / 2f)) - (RadioButtonSize / 2f), chosen);
            if (labelClicked && !radioClicked && !chosen) { SoundDefOf.RadioButtonClicked.PlayOneShotOnCamera(); }

            return enabled && labelClicked;
        }

        public static bool CheckboxPartial(Rect rect, string label, ref MultiCheckboxState state, string tooltip = null, bool enabled = true, bool allowPartialInCycle = true)
        {
            var prevAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, label);

            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                if (!tooltip.NullOrEmpty()) { TooltipHandler.TipRegion(rect, tooltip); }
            }

            var result = false;
            if (enabled && Widgets.ButtonInvisible(rect))
            {
                if (state == MultiCheckboxState.Off)
                {
                    state = allowPartialInCycle ? MultiCheckboxState.Partial : MultiCheckboxState.On;
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                }
                else if (state == MultiCheckboxState.Partial)
                {
                    state = MultiCheckboxState.On;
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                }
                else
                {
                    state = MultiCheckboxState.Off;
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                }

                result = true;
            }

            var prevColor = GUI.color;
            if (!enabled) { GUI.color = InactiveColor; }

            Texture2D image;
            if (state == MultiCheckboxState.Partial) { image = Widgets.CheckboxPartialTex; }
            else if (state == MultiCheckboxState.On) { image = Widgets.CheckboxOnTex; }
            else { image = Widgets.CheckboxOffTex; }

            GUI.DrawTexture(new Rect((rect.x + rect.width) - CheckboxSize, rect.y, CheckboxSize, CheckboxSize), image);
            if (!enabled) { GUI.color = prevColor; }

            Text.Anchor = prevAnchor;

            return result;
        }

        public static string TextEntryLabeled(Rect rect, string label, string text, string tooltip = null)
        {
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                if (!tooltip.NullOrEmpty()) { TooltipHandler.TipRegion(rect, tooltip); }
            }

            var rect2 = rect.LeftHalf().Rounded();
            var rect3 = rect.RightHalf().Rounded();
            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect2, label);
            Text.Anchor = anchor;

            return rect.height <= ButtonSize ? Widgets.TextField(rect3, text) : Widgets.TextArea(rect3, text);
        }

        public static void DoAddonsListing(Listing_StandardPlus listing, Rules rules, bool editMode)
        {
            foreach (var addon in rules.GetAddons())
            {
                if (rules.IsPreset && !addon.AllowedInPreset) { continue; }

                if (addon.Widget == OptionWidget.Checkbox)
                {
                    var value = rules.GetAddonValue(addon, (bool) addon.DefaultValue);
                    if (!listing.CheckboxLabeled(addon.Label, ref value, addon.Tooltip, editMode)) { continue; }
                    if (rules.IsPreset) { addon.Handle.ChangePresetValue(rules, value); }
                    else { addon.Handle.ChangeValue(rules.Pawn, value); }
                }
                else if (addon.Widget == OptionWidget.TextEntry)
                {
                    if (addon.Type == typeof(string))
                    {
                        var oldValue = rules.GetAddonValue(addon, (string) addon.DefaultValue);
                        var newValue = listing.TextEntryLabeled(addon.Label, oldValue, addon.Tooltip);
                        if (!editMode || oldValue.Equals(newValue)) { continue; }

                        if (rules.IsPreset) { addon.Handle.ChangePresetValue(rules, newValue); }
                        else { addon.Handle.ChangeValue(rules.Pawn, newValue); }
                    }
                    else if (addon.Type == typeof(int))
                    {
                        var oldValue = rules.GetAddonValue(addon, (int) addon.DefaultValue).ToString();
                        var newValue = listing.TextEntryLabeled(addon.Label, oldValue, addon.Tooltip);
                        if (!editMode || oldValue.Equals(newValue)) { continue; }

                        if (rules.IsPreset) { addon.Handle.ChangePresetValue(rules, newValue.ToInt()); }
                        else { addon.Handle.ChangeValue(rules.Pawn, newValue.ToInt()); }
                    }
                    else if (addon.Type == typeof(float))
                    {
                        var oldValue = rules.GetAddonValue(addon, (float) addon.DefaultValue).ToString(CultureInfo.InvariantCulture);
                        var newValue = listing.TextEntryLabeled(addon.Label, oldValue, addon.Tooltip);
                        if (!editMode || oldValue.Equals(newValue)) { continue; }

                        if (rules.IsPreset) { addon.Handle.ChangePresetValue(rules, newValue.ToFloat()); }
                        else { addon.Handle.ChangeValue(rules.Pawn, newValue.ToFloat()); }
                    }
                }
                else if (addon.Widget == OptionWidget.Button)
                {
                    if (!listing.ButtonText(addon.Label, addon.Tooltip) && editMode) { continue; }

                    if (rules.IsPreset) { addon.Handle.DoDefaultClick(rules.Type.AsTarget); }
                    else { addon.Handle.DoClick(rules.Pawn); }
                }
            }
        }
    }
}
