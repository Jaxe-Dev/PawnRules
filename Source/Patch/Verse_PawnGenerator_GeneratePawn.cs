using Harmony;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", typeof(PawnKindDef), typeof(Faction))]
    internal static class Verse_PawnGenerator_GeneratePawn
    {
        private static void Postfix(ref Pawn __result)
        {
            if (!Registry.IsActive) { return; }

            if ((__result == null) || ((!__result.Faction?.IsPlayer ?? false) && (!__result.HostFaction?.IsPlayer ?? false))) { return; }

            Registry.GetOrDefaultRules(__result);
        }
    }
}
