using Harmony;
using PawnRules.Data;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(WorkGiver_InteractAnimal), "TakeFoodForAnimalInteractJob")]
    internal static class RimWorld_WorkGiver_InteractAnimal_TakeFoodForAnimalInteractJob
    {
        private static bool Prefix(ref Job __result, Pawn pawn, Pawn tamee)
        {
            if (!Registry.IsActive || !Registry.AllowTrainingFood) { return true; }

            var required = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee) * 2f * 4f;

            Registry.ExemptedTrainer = pawn;
            var foodSource = FoodUtility.BestFoodSourceOnMap(pawn, tamee, false, out var foodDef, FoodPreferability.RawTasty, false, false, false, false, false);

            if (foodSource == null)
            {
                __result = null;
                return false;
            }

            var nutrition = FoodUtility.GetNutrition(foodSource, foodDef);

            __result = new Job(JobDefOf.TakeInventory, foodSource) { count = Mathf.CeilToInt(required / nutrition) };
            return false;
        }
    }
}
