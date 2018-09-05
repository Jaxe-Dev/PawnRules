using Harmony;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(RelationsUtility), nameof(RelationsUtility.TryDevelopBondRelation))]
    internal static class RimWorld_RelationsUtility_TryDevelopBondRelation
    {
        private static bool Prefix(ref bool __result, Pawn humanlike, Pawn animal)
        {
            if (!Registry.IsActive || (humanlike == null) || (animal == null) || !humanlike.CanHaveRules()) { return true; }

            var rules = Registry.GetRules(humanlike);
            var restriction = rules?.GetRestriction(RestrictionType.Bonding);
            if ((restriction == null) || restriction.IsVoid || restriction.Allows(animal.def)) { return true; }

            __result = false;
            return false;
        }
    }
}
