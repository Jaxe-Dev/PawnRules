using Harmony;
using PawnRules.Data;
using RimWorld;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(Pawn_FoodRestrictionTracker), "Configurable", MethodType.Getter)]
    internal static class RimWorld_Pawn_FoodRestrictionTracker_Configurable
    {
        private static bool Prefix(ref bool __result)
        {
            if (!Registry.IsActive) { return true; }

            __result = false;
            return false;
        }
    }
}
