using System;
using Harmony;
using RimWorld;
using Verse;

namespace PawnRules.Patch {
    [HarmonyPatch(typeof(PawnColumnWorker_FoodRestriction), "Compare")]
    internal static class RimWorld_PawnColumnWorker_FoodRestriction_Compare
    {
        private static bool Prefix(ref int __result, Pawn a, Pawn b)
        {
            __result = string.Compare(a.GetRules().Name, b.GetRules().Name, StringComparison.OrdinalIgnoreCase);
            return false;
        }
    }
}
