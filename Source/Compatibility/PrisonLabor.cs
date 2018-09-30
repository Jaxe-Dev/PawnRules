using System.Linq;
using System.Reflection;
using Harmony;
using PawnRules.Data;
using PawnRules.Patch;
using RimWorld;
using Verse;

namespace PawnRules.Compatibility
{
    internal static class PrisonLabor
    {
        private const bool AllowHarvest = true;
        private const bool ForceScanWholeMap = true;
        public static void Setup()
        {
            var assembly = GetAssembly("PrisonLabor");
            var foodTweaks = assembly?.GetType("FoodUtility_Tweak");
            if (foodTweaks == null) { return; }

            var tryFindBestFoodSourceFor = AccessTools.Method(foodTweaks, "TryFindBestFoodSourceFor");
            var bestFoodInInventory = AccessTools.Method(foodTweaks, "BestFoodInInventory");
            var bestFoodSourceOnMap = AccessTools.Method(foodTweaks, "BestFoodSourceOnMap");

            if ((tryFindBestFoodSourceFor == null) || (bestFoodInInventory == null) || (bestFoodSourceOnMap == null)) { return; }

            Patcher.Harmony.Patch(tryFindBestFoodSourceFor, new HarmonyMethod(typeof(PrisonLabor), nameof(TryFindBestFoodSourceFor)));
            Patcher.Harmony.Patch(bestFoodInInventory, new HarmonyMethod(typeof(PrisonLabor), nameof(BestFoodInInventory)));
            Patcher.Harmony.Patch(bestFoodSourceOnMap, new HarmonyMethod(typeof(PrisonLabor), nameof(BestFoodSourceOnMap)));

            Mod.Warning("Compatibility patch added for Prison Labor");
        }

        private static bool TryFindBestFoodSourceFor(ref bool __result, Pawn getter, Pawn eater, bool desperate, out Thing foodSource, out ThingDef foodDef, bool canRefillDispenser = true, bool canUseInventory = true, bool allowForbidden = false, bool allowCorpse = true, bool allowSociallyImproper = false)
        {
            foodSource = null;
            foodDef = null;

            var restriction = Registry.GetRules(eater)?.GetRestriction(RestrictionType.Food);
            if (eater.InMentalState || (restriction == null) || restriction.IsVoid) { return true; }

            __result = FoodUtility.TryFindBestFoodSourceFor(getter, eater, desperate, out foodSource, out foodDef, canRefillDispenser, canUseInventory, allowForbidden, allowCorpse, allowSociallyImproper, AllowHarvest, ForceScanWholeMap);
            return false;
        }

        private static bool BestFoodInInventory(ref Thing __result, Pawn holder, Pawn eater = null, FoodPreferability minFoodPref = FoodPreferability.NeverForNutrition, FoodPreferability maxFoodPref = FoodPreferability.MealLavish, float minStackNutrition = 0f, bool allowDrug = false)
        {
            if (eater == null) { eater = holder; }

            var restriction = Registry.GetRules(eater)?.GetRestriction(RestrictionType.Food);
            if (eater.InMentalState || (restriction == null) || restriction.IsVoid) { return true; }

            __result = FoodUtility.BestFoodInInventory(holder, eater, minFoodPref, maxFoodPref, minStackNutrition, allowDrug);
            return false;
        }

        private static bool BestFoodSourceOnMap(ref Thing __result, Pawn getter, Pawn eater, bool desperate, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false, bool allowSociallyImproper = false)
        {
            var restriction = Registry.GetRules(eater)?.GetRestriction(RestrictionType.Food);
            if (eater.InMentalState || (restriction == null) || restriction.IsVoid) { return true; }

            __result = FoodUtility.BestFoodSourceOnMap(getter, eater, desperate, out var _, maxPref, allowPlant, allowDrug, allowCorpse, allowDispenserFull, allowDispenserEmpty, allowForbidden, allowSociallyImproper, AllowHarvest, ForceScanWholeMap);
            return false;
        }

        private static Assembly GetAssembly(string name) => LoadedModManager.RunningMods.Select(mod => mod.assemblies.loadedAssemblies.FirstOrDefault(assembly => assembly.GetName().Name == name)).FirstOrDefault(assembly => assembly != null);
    }
}
