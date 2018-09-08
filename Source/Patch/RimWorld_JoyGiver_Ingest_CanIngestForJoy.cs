using Harmony;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(JoyGiver_Ingest), "CanIngestForJoy")]
    internal static class RimWorld_JoyGiver_Ingest_CanIngestForJoy
    {
        private static bool Prefix(ref bool __result, Pawn pawn, Thing t)
        {
            var rules = Registry.GetRules(pawn);
            var restriction = rules?.GetRestriction(RestrictionType.Food);
            if (pawn.InMentalState || (restriction == null) || restriction.IsVoid || restriction.Allows(t.def)) { return true; }

            __result = false;
            return false;
        }
    }
}
