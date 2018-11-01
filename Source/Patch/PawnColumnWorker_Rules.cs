using PawnRules.Data;
using PawnRules.Interface;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnRules.Patch
{
    public class PawnColumnWorker_Rules : PawnColumnWorker
    {
        private const int TopAreaHeight = 65;

        public override void DoHeader(Rect rect, PawnTable table)
        {
            base.DoHeader(rect, table);
            var headerRect = new Rect(rect.x, rect.y + (rect.height - TopAreaHeight), Mathf.Min(rect.width, 360f), 32f);
            if (Widgets.ButtonText(headerRect, "Global Rules")) { Dialog_Global.Open(); }
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (!Registry.IsActive) { return; }

            var rules = Registry.GetOrNewRules(pawn);
            if (rules == null) { return; }

            var selectorRect = new Rect(rect.x, rect.y + 2f, rect.width, rect.height - 4f);
            if (Widgets.ButtonText(selectorRect, rules.GetDisplayName())) { Dialog_Rules.Open(pawn); }
        }

        public override int GetMinWidth(PawnTable table) => Mathf.Max(base.GetMinWidth(table), Mathf.CeilToInt(194f));

        public override int GetOptimalWidth(PawnTable table) => Mathf.Clamp(Mathf.CeilToInt(251f), GetMinWidth(table), GetMaxWidth(table));

        public override int GetMinHeaderHeight(PawnTable table) => Mathf.Max(base.GetMinHeaderHeight(table), TopAreaHeight);

        public override int Compare(Pawn a, Pawn b) => 0;
    }
}
