using System.Collections;
using System.Collections.Generic;
using PawnRules.Data;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnRules.Patch
{
    internal static class Extensions
    {
        public static string Italic(this string self) => "<i>" + self + "</i>";
        public static string Bold(this string self) => "<b>" + self + "</b>";

        public static int LastIndex(this IList self) => self.Count - 1;
        public static int ToInt(this string self, int defaultValue = 0) => int.TryParse(self, out var result) ? result : defaultValue;
        public static float ToFloat(this string self, float defaultValue = 0f) => float.TryParse(self, out var result) ? result : defaultValue;

        public static bool CanHaveRules(this Pawn self) => (self != null) && !self.Dead && (self.GetTargetType() != null);
        public static PawnType GetTargetType(this Pawn self)
        {
            if (self == null) { return null; }
            if ((self.Faction == Faction.OfPlayer) && self.IsColonist) { return PawnType.Colonist; }
            if ((self.Faction == Faction.OfPlayer) && self.RaceProps.Animal) { return PawnType.Animal; }
            if (self.HostFaction == Faction.OfPlayer) { return self.IsPrisonerOfColony ? PawnType.Prisoner : PawnType.Guest; }
            return null;
        }

        public static Rules GetRules(this Pawn self) => Registry.GetRules(self);

        public static string GetDisplayName(this Presetable self) => self == null ? Lang.Get("Preset.None") : self.Name ?? Lang.Get("Preset.Personalized");

        public static Rect AdjustedBy(this Rect self, float x, float y, float width, float height) => new Rect(self.x + x, self.y + y, self.width + width, self.height + height);
        public static Rect Round(this Rect self) => new Rect(Mathf.Round(self.x), Mathf.Round(self.y), Mathf.Round(self.width), Mathf.Round(self.height));

        public static Rect[] GetHGrid(this Rect self, float padding, params float[] widths)
        {
            var unfixedCount = 0;
            var currentX = self.x;
            var fixedWidths = 0f;

            var rects = new List<Rect> { self };

            for (var index = 0; index < widths.Length; index++)
            {
                var width = widths[index];
                if (width >= 0f) { fixedWidths += width; }
                else { unfixedCount++; }

                if (index != widths.LastIndex()) { fixedWidths += padding; }
            }

            var unfixedWidth = unfixedCount > 0 ? Mathf.Max(0f, (self.width - fixedWidths) / unfixedCount) : 0f;

            foreach (var width in widths)
            {
                float newWidth;

                if (width >= 0f)
                {
                    newWidth = width;
                    rects.Add(new Rect(currentX, self.y, newWidth, self.height).Round());
                }
                else
                {
                    newWidth = unfixedWidth;
                    rects.Add(new Rect(currentX, self.y, newWidth, self.height).Round());
                }

                currentX = Mathf.Min(self.xMax, currentX + newWidth + (newWidth > 0f ? padding : 0f));
            }

            return rects.ToArray();
        }
        public static Rect[] GetVGrid(this Rect self, float padding, params float[] heights)
        {
            var unfixedCount = 0;
            var currentY = self.y;
            var fixedHeights = 0f;

            var rects = new List<Rect> { self };

            for (var index = 0; index < heights.Length; index++)
            {
                var height = heights[index];
                if (height >= 0f) { fixedHeights += height; }
                else { unfixedCount++; }

                if (index != heights.LastIndex()) { fixedHeights += padding; }
            }

            var unfixedHeight = unfixedCount > 0 ? Mathf.Max(0f, (self.height - fixedHeights) / unfixedCount) : 0f;

            foreach (var height in heights)
            {
                float newHeight;

                if (height >= 0f)
                {
                    newHeight = height;
                    rects.Add(new Rect(self.x, currentY, self.width, newHeight).Round());
                }
                else
                {
                    newHeight = unfixedHeight;
                    rects.Add(new Rect(self.x, currentY, self.width, newHeight).Round());
                }

                currentY = Mathf.Min(self.yMax, currentY + newHeight + (newHeight > 0f ? padding : 0f));
            }

            return rects.ToArray();
        }

    }
}
