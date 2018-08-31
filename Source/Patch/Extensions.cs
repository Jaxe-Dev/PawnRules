using System;
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

        public static string GetCategoryLabel(this ThingDef self) => self.category == ThingCategory.Item ? self.FirstThingCategory.LabelCap : self.category.ToString();
        public static Rect AdjustedBy(this Rect self, float x, float y, float width, float height) => new Rect(self.x + x, self.y + y, self.width + width, self.height + height);

        public static int LastIndex(this Array self) => self.Length - 1;

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

        public static Rect[] GetHGrid(this Rect self, float spacing, params float[] widths)
        {
            var unfixedCount = 0;
            var currentX = self.x;
            var fixedWidths = 0f;
            var rects = new Rect[widths.Length];

            for (var index = 0; index < widths.Length; index++)
            {
                var width = widths[index];
                if (width > 0) { fixedWidths += width; }
                else { unfixedCount++; }

                if (index != widths.LastIndex()) { fixedWidths += spacing; }
            }

            var unfixedWidth = unfixedCount > 0 ? (self.width - fixedWidths) / unfixedCount : 0;

            for (var index = 0; index < widths.Length; index++)
            {
                var width = widths[index];
                float newWidth;
                if (width > 0)
                {
                    newWidth = width;
                    rects[index] = new Rect(currentX, self.y, newWidth, self.height);
                }
                else
                {
                    newWidth = unfixedWidth;
                    rects[index] = new Rect(currentX, self.y, newWidth, self.height);
                }
                currentX += newWidth + spacing;
            }

            return rects;
        }

        public static Rect[] GetVGrid(this Rect self, float spacing, params float[] heights)
        {
            var unfixedCount = 0;
            var currentY = self.y;
            var fixedHeights = 0f;
            var rects = new Rect[heights.Length];

            for (var index = 0; index < heights.Length; index++)
            {
                var height = heights[index];
                if (height > 0) { fixedHeights += height; }
                else { unfixedCount++; }

                if (index != heights.LastIndex()) { fixedHeights += spacing; }
            }

            var unfixedWidth = unfixedCount > 0 ? (self.height - fixedHeights) / unfixedCount : 0;

            for (var index = 0; index < heights.Length; index++)
            {
                var height = heights[index];
                float newHeight;
                if (height > 0)
                {
                    newHeight = height;
                    rects[index] = new Rect(self.x, currentY, self.width, newHeight);
                }
                else
                {
                    newHeight = unfixedWidth;
                    rects[index] = new Rect(self.x, currentY, self.width, newHeight);
                }
                currentY += newHeight + spacing;
            }

            return rects;
        }
        /*
                public static Rect[] GetVGrid(this Rect self, float spacing, params float[] heights)
                {
                    var unfixedCount = 0;
                    var currentY = self.y;
                    var fixedHeights = 0f;
                    var rects = new Rect[heights.Length];

                    foreach (var height in heights)
                    {
                        if (height > 0) { fixedHeights += height - (spacing * 2); }
                        else { unfixedCount++; }
                    }

                    var unfixedHeight = unfixedCount > 0 ? (self.height - fixedHeights) / unfixedCount : 0;

                    for (var index = 0; index < heights.Length; index++)
                    {
                        var height = heights[index];
                        float newHeight;
                        if (height > 0)
                        {
                            newHeight = height;
                            rects[index] = new Rect(self.x, currentY, self.width, newHeight);
                        }
                        else
                        {
                            newHeight = unfixedHeight;
                            rects[index] = new Rect(self.x, currentY, self.width, newHeight);
                        }
                        currentY += newHeight + spacing;
                    }

                    return rects;
                }
        */
    }
}
