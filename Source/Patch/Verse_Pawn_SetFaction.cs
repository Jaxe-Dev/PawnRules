using HarmonyLib;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(Pawn), "SetFaction")]
    internal static class Verse_Pawn_SetFaction
    {
        private static void Prefix(Pawn __instance, Faction newFaction)
        {
            if (!Registry.IsActive) { return; }
            Registry.FactionUpdate(__instance, newFaction);
        }
    }
}
