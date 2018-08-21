using Harmony;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(RelationsUtility), nameof(RelationsUtility.TryDevelopBondRelation))]
    internal static class RimWorld_RelationsUtility_TryDevelopBondRelation
    {
        private static bool Prefix(Pawn humanlike, Pawn animal)
        {
            if (!Registry.IsActive || (humanlike == null) || (animal == null) || !humanlike.CanHaveRules()) { return true; }

            var rules = Registry.GetRules(humanlike);
            if (rules == null) { return true; }
            var restrictions = rules.GetRestriction(RestrictionType.Bonding);
            return (restrictions == null) || restrictions.IsVoid || restrictions.Allows(animal.def);
        }
    }
}
