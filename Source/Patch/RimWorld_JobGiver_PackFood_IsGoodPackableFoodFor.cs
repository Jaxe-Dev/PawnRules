using Harmony;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(JobGiver_PackFood), "IsGoodPackableFoodFor")]
    internal static class RimWorld_JobGiver_PackFood_IsGoodPackableFoodFor
    {
        private static void Postfix(ref bool __result, Thing food, Pawn forPawn)
        {
            if (!Registry.IsActive) { return; }

            var restriction = Registry.GetRules(forPawn)?.GetRestriction(RestrictionType.Food);
            if (forPawn.InMentalState || (restriction == null) || restriction.IsVoid) { return; }

            __result = __result && restriction.AllowsFood(food.def, forPawn);
        }
    }
}
