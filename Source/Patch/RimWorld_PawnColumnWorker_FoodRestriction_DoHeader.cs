using Harmony;
using PawnRules.Data;
using PawnRules.Interface;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(PawnColumnWorker_FoodRestriction), "DoHeader")]
    internal static class RimWorld_PawnColumnWorker_FoodRestriction_DoHeader
    {
        private static readonly PawnColumnDef RulesColumnDef = new PawnColumnDef
        {
            label = Lang.Get("PresetType.Rules"),
            sortable = true,
            workerClass = typeof(PawnColumnWorker_FoodRestriction)
        };
        private static readonly DummyColumnWorker ColumnWorker = new DummyColumnWorker { def = RulesColumnDef };

        private static bool Prefix(Rect rect, PawnTable table)
        {
            ColumnWorker.DoHeaderDummy(rect, table);
            var buttonRect = new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f);
            if (Widgets.ButtonText(buttonRect, Lang.Get("Dialog_Rules.OpenDefaults"))) { Dialog_Rules.OpenDefaults(PawnType.Colonist); }

            return false;
        }

        private class DummyColumnWorker : PawnColumnWorker
        {
            public void DoHeaderDummy(Rect rect, PawnTable table) => base.DoHeader(rect, table);

            public override void DoCell(Rect rect, Pawn pawn, PawnTable table) { }
        }
    }
}
