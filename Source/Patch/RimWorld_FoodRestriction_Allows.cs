using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    internal static class RimWorld_FoodRestriction_Allows
    {
        [HarmonyPatch(typeof(FoodRestriction), "Allows", typeof(ThingDef))]
        public static class ByThing
        {
            private static void Postfix(ref bool __result) => __result = true;
        }

        [HarmonyPatch(typeof(FoodRestriction), "Allows", typeof(Thing))]
        private static class ByThingDef
        {
            public static void Postfix(ref bool __result) => __result = true;
        }
    }
}
