using HarmonyLib;
using PawnRules.Data;
using RimWorld;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(Pawn_FoodRestrictionTracker), "CurrentFoodRestriction", MethodType.Getter)]
    internal static class RimWorld_Pawn_FoodRestrictionTracker_CurrentFoodRestriction
    {
        private static bool Prefix(ref FoodRestriction __result)
        {
            if (!Registry.IsActive) { return true; }

            __result = new FoodRestriction(-1, Lang.Get("Gizmo.EditRulesLabel"));
            return false;
        }
    }
}
