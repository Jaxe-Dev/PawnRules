using System.Linq;
using Harmony;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(WorkGiver_InteractAnimal), "HasFoodToInteractAnimal")]
    internal static class RimWorld_WorkGiver_InteractAnimal_HasFoodToInteractAnimal
    {
        private static bool Prefix(ref bool __result, Pawn pawn, Pawn tamee)
        {
            if (!Registry.IsActive || Registry.AllowTrainingFood) { return true; }

            var restriction = Registry.GetRules(tamee)?.GetRestriction(RestrictionType.Food);
            if ((restriction == null) || restriction.IsVoid) { return true; }

            var innerContainer = pawn.inventory.innerContainer;
            var requiredNutritionPerFeed = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee);

            var count = 0;
            var nutrition = 0.0f;

            foreach (var thing in innerContainer.ToArray())
            {
                if (!tamee.RaceProps.CanEverEat(thing) || (thing.def.ingestible.preferability > FoodPreferability.RawTasty) || thing.def.IsDrug || !restriction.Allows(thing.def)) { continue; }

                for (var index = 0; index < thing.stackCount; ++index)
                {
                    nutrition += thing.GetStatValue(StatDefOf.Nutrition);

                    if (nutrition >= (double) requiredNutritionPerFeed)
                    {
                        count++;
                        nutrition = 0.0f;
                    }

                    if (count < 2) { continue; }

                    __result = true;
                    return false;
                }
            }

            __result = false;
            return false;
        }
    }
}
