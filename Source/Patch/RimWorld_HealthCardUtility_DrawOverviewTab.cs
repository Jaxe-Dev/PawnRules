using HarmonyLib;
using PawnRules.Data;
using PawnRules.Interface;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(HealthCardUtility), "DrawOverviewTab")]
    internal static class RimWorld_HealthCardUtility_DrawOverviewTab
    {
        private static void Postfix(ref float __result, UnityEngine.Rect leftRect, Pawn pawn, float curY)
        {
            if (!Registry.IsActive) { return; }

            FloatMenu overviewWindowStack = Find.WindowStack.FloatMenu;

            if (overviewWindowStack == null) { return; }

            List<FloatMenuOption> overviewWindowStackOptions = (List<FloatMenuOption>) typeof(FloatMenu).GetField("options", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(overviewWindowStack);
            if (overviewWindowStackOptions?.Count > 0 && overviewWindowStackOptions.Last().Label.Equals("ManageFoodRestrictions".Translate()))
            {
                overviewWindowStack.Close(false);
                Dialog_Rules.Open(pawn);
            }
        }
    }
}
