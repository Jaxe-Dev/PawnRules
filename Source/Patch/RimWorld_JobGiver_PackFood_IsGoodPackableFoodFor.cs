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

            var rules = Registry.GetRules(forPawn);
            if (forPawn.InMentalState || (rules == null) || rules.GetRestriction(RestrictionType.Food).IsVoid) { return; }

            __result = __result && rules.GetRestriction(RestrictionType.Food).Allows(food.def);
        }
    }
}
