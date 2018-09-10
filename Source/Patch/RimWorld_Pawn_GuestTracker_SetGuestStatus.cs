using Harmony;
using PawnRules.Data;
using RimWorld;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(Pawn_GuestTracker), "SetGuestStatus")]
    internal static class RimWorld_Pawn_GuestTracker_SetGuestStatus
    {
        private static void Prefix(Pawn_GuestTracker __instance, Faction newHost, bool prisoner = false)
        {
            if (!Registry.IsActive) { return; }

            Registry.FactionUpdate(Access.Field_RimWorld_Pawn_GuestTracker_Pawn_Get(__instance), newHost, !prisoner);
        }
    }
}
