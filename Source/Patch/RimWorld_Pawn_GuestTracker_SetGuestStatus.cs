using Harmony;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(Pawn_GuestTracker), "SetGuestStatus")]
    internal static class RimWorld_Pawn_GuestTracker_SetGuestStatus
    {
        private static void Prefix(Pawn_GuestTracker __instance, Faction newHost, bool prisoner = false)
        {
            if (!Registry.IsActive) { return; }
            Registry.FactionUpdate(Traverse.Create(__instance).Field("pawn").GetValue<Pawn>(), newHost, !prisoner);
        }
    }
}
