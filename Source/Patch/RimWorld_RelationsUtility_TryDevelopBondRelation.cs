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
            if (!Registry.IsActive || !humanlike.CanHaveRules()) { return true; }

            var rules = Registry.GetRules(humanlike);
            var restrictions = rules.GetRestriction(RestrictionType.Bonding);
            return (rules == null) || (restrictions.IsVoid) || restrictions.Allows(animal.def);
        }
    }
}
