using System.Collections.Generic;
using Harmony;
using PawnRules.Data;
using PawnRules.Interface;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    internal static class Verse_Pawn_GetGizmos
    {
        private static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if (!Registry.IsActive || (Find.Selector.NumSelected != 1) || (__instance == null) || !__instance.CanHaveRules()) { return; }
            __result = new List<Gizmo>(__result) { GuiPlus.EditRulesCommand(__instance) };
        }
    }
}
