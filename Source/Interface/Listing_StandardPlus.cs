using UnityEngine;
using Verse;

namespace PawnRules.Interface
{
    internal class Listing_StandardPlus : Listing_Standard
    {
        private const float ButtonHeight = 30f;
        private const float ScrollbarSize = 20f;
        private const float MaxScrollViewHeight = 999999f;

        private Vector2 _scrollPosition;

        private bool _scrollable;

        public override void Begin(Rect rect) { Begin(rect, false); }
        public void Begin(Rect rect, bool scrollable)
        {
            _scrollable = scrollable;
            var listRect = rect;

            if (_scrollable)
            {
                var viewRect = new Rect(0f, 0f, rect.width - ScrollbarSize, CurHeight);

                Widgets.BeginScrollView(rect, ref _scrollPosition, viewRect);
                listRect = viewRect.AtZero();
                listRect.height = MaxScrollViewHeight;
            }

            base.Begin(listRect);
        }

        public override void End()
        {
            base.End();
            if (_scrollable) { Widgets.EndScrollView(); }
        }

        public void NewColumn(float spacing, float width)
        {
            curY = 0f;
            curX += ColumnWidth + spacing;
            ColumnWidth = width;
        }

        public void LabelMedium(string label)
        {
            var font = Text.Font;
            Text.Font = GameFont.Medium;
            Label(label);
            Text.Font = font;
        }

        public void LabelTiny(string label)
        {
            var font = Text.Font;
            Text.Font = GameFont.Tiny;
            Label(label);
            Text.Font = font;
        }

        public bool ButtonText(string label, string tooltip = null, bool enabled = true)
        {
            var result = GuiPlus.ButtonText(GetRect(ButtonHeight), label, tooltip, enabled);
            Gap(verticalSpacing);
            return result;
        }

        public bool CheckboxLabeled(string label, ref bool checkOn, string tooltip = null, bool enabled = true)
        {
            var value = checkOn;
            base.CheckboxLabeled(label, ref value, tooltip);

            if (!enabled) { return checkOn; }

            var result = checkOn != value;
            checkOn = value;
            return result;
        }

        public bool CheckboxPartial(string label, ref MultiCheckboxState state, string tooltip = null, bool enabled = true, bool allowPartialInCycle = false)
        {
            var result = GuiPlus.CheckboxPartial(GetRect(Text.LineHeight), label, ref state, tooltip, enabled, allowPartialInCycle);
            Gap(verticalSpacing);

            return result;
        }

        public string TextEntryLabeled(string label, string text, string tooltip = null, int lineCount = 1)
        {
            var rect = GetRect(Text.LineHeight * lineCount);
            var result = GuiPlus.TextEntryLabeled(rect, label, text, tooltip);
            Gap(verticalSpacing);
            return result;
        }

        public bool RadioButtonInverted(string label, bool active, string tooltip = null, bool enabled = true)
        {
            var lineHeight = Text.LineHeight;
            var rect = GetRect(lineHeight);
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(rect)) { Widgets.DrawHighlight(rect); }
                TooltipHandler.TipRegion(rect, tooltip);
            }
            var result = GuiPlus.RadioButtonInverted(rect, label, active, enabled);
            Gap(verticalSpacing);
            return result;
        }
    }
}
