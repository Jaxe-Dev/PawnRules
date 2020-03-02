using HarmonyLib;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    internal static class Verse_PawnGenerator_GeneratePawn
    {
        [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", typeof(PawnKindDef), typeof(Faction))]
        private static class ByRequest
        {
            public static void Postfix(ref Pawn __result)
            {
                if (!Registry.IsActive) { return; }

                if ((__result == null) || ((!__result.Faction?.IsPlayer ?? true) && (!__result.HostFaction?.IsPlayer ?? true))) { return; }

                Registry.GetOrDefaultRules(__result);
            }
        }

        [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", typeof(PawnGenerationRequest))]
        private static class ByOther
        {
            public static void Postfix(ref Pawn __result) => ByRequest.Postfix(ref __result);
        }
    }
}
