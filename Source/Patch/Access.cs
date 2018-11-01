using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnRules.Patch
{
    internal static class Access
    {
        private static readonly MethodInfo Method_RimWorld_FoodUtility_GetMaxRegionsToScan = AccessTools.Method(typeof(FoodUtility), "GetMaxRegionsToScan");
        private static readonly MethodInfo Method_RimWorld_FoodUtility_IsFoodSourceOnMapSociallyProper = AccessTools.Method(typeof(FoodUtility), "IsFoodSourceOnMapSociallyProper");
        private static readonly MethodInfo Method_RimWorld_FoodUtility_SpawnedFoodSearchInnerScan = AccessTools.Method(typeof(FoodUtility), "SpawnedFoodSearchInnerScan");
        private static readonly FieldInfo Field_RimWorld_FoodUtility_Filtered = AccessTools.Field(typeof(FoodUtility), "filtered");
        private static readonly FieldInfo Field_Verse_LoadedModManager_RunningMods = AccessTools.Field(typeof(LoadedModManager), "runningMods");

        public static int Method_RimWorld_FoodUtility_GetMaxRegionsToScan_Call(Pawn getter, bool forceScanWholeMap) => (int) Method_RimWorld_FoodUtility_GetMaxRegionsToScan.Invoke(null, new object[] { getter, forceScanWholeMap });
        public static bool Method_RimWorld_FoodUtility_IsFoodSourceOnMapSociallyProper_Call(Thing thing, Pawn getter, Pawn eater, bool allowSociallyImproper) => (bool) Method_RimWorld_FoodUtility_IsFoodSourceOnMapSociallyProper.Invoke(null, new object[] { thing, getter, eater, allowSociallyImproper });
        public static Thing Method_RimWorld_FoodUtility_SpawnedFoodSearchInnerScan_Call(Pawn eater, IntVec3 root, List<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null) => (Thing) Method_RimWorld_FoodUtility_SpawnedFoodSearchInnerScan.Invoke(null, new object[] { eater, root, searchSet, peMode, traverseParams, maxDistance, validator });
        public static HashSet<Thing> Field_RimWorld_FoodUtility_Filtered_Get() => (HashSet<Thing>) Field_RimWorld_FoodUtility_Filtered.GetValue(null);
        public static List<ModContentPack> Field_Verse_LoadedModManager_RunningMods_Get() => (List<ModContentPack>) Field_Verse_LoadedModManager_RunningMods.GetValue(null);
    }
}
