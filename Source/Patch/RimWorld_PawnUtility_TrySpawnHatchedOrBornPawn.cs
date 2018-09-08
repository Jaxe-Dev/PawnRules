using Harmony;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(PawnUtility), "TrySpawnHatchedOrBornPawn")]
    internal static class RimWorld_PawnUtility_TrySpawnHatchedOrBornPawn
    {
        private static void Postfix(bool __result, Pawn pawn, Thing motherOrEgg)
        {
            if (!Registry.IsActive || !__result || (pawn == null) || !(motherOrEgg is Pawn mother) || (mother == null) || (mother.Faction != Faction.OfPlayer)) { return; }
            Registry.CloneRules(mother, pawn);
        }
    }
}
