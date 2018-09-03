using Harmony;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), typeof(PawnGenerationRequest))]
    internal static class Verse_PawnGenerator_GeneratePawn
    {
        private static void Postfix(ref Pawn __result)
        {
            if (!Registry.IsActive) { return; }

            if ((__result == null) || ((__result.Faction != Faction.OfPlayer) && (__result.HostFaction != Faction.OfPlayer))) { return; }

            Registry.GetOrDefaultRules(__result);
        }
    }
}
