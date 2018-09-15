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
            var restriction = Registry.GetRules(pawn)?.GetRestriction(RestrictionType.Food);
            if (pawn.InMentalState || (restriction == null) || restriction.IsVoid || restriction.AllowsFood(t.def, pawn)) { return true; }

            __result = false;
            return false;
        }
    }
}
