using Harmony;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFactionDirect))]
    internal static class Verse_Pawn_SetFactionDirect
    {
        private static void Prefix(Pawn __instance, Faction newFaction)
        {
            if (!Registry.IsActive) { return; }
            Registry.FactionUpdate(__instance, newFaction);
        }
    }
}
