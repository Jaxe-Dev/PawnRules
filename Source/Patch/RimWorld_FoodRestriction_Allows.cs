using Harmony;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    internal static class RimWorld_FoodRestriction_Allows
    {
        [HarmonyPatch(typeof(FoodRestriction), "Allows", typeof(ThingDef))]
        public static class FromThing
        {
            private static void Postfix(ref bool __result) => __result = true;
        }

        [HarmonyPatch(typeof(FoodRestriction), "Allows", typeof(Thing))]
        private static class FromThingDef
        {
            public static void Postfix(ref bool __result) => __result = true;
        }
    }
}
