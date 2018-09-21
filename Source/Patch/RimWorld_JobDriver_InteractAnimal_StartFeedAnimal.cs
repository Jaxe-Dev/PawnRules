using Harmony;
using PawnRules.Data;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(JobDriver_InteractAnimal), "StartFeedAnimal")]
    internal static class RimWorld_JobDriver_InteractAnimal_StartFeedAnimal
    {
        private static bool Prefix(ref Toil __result, JobDriver_InteractAnimal __instance, TargetIndex tameeInd)
        {
            var toil = new Toil();
            toil.initAction = () =>
                              {
                                  var feedNutritionLeft = Traverse.Create(__instance).Field<float>("feedNutritionLeft");

                                  var actor = toil.GetActor();
                                  var target = (Pawn) (Thing) actor.CurJob.GetTarget(tameeInd);

                                  PawnUtility.ForceWait(target, 270, actor);

                                  Registry.ExemptedTrainer = actor;

                                  var bestFoodInInventory = FoodUtility.BestFoodInInventory(actor, target, FoodPreferability.NeverForNutrition, FoodPreferability.RawTasty);

                                  if (bestFoodInInventory == null) { actor.jobs.EndCurrentJob(JobCondition.Incompletable); }
                                  else
                                  {
                                      actor.mindState.lastInventoryRawFoodUseTick = Find.TickManager.TicksGame;

                                      var stackCountForNutrition = FoodUtility.StackCountForNutrition(feedNutritionLeft.Value, bestFoodInInventory.GetStatValue(StatDefOf.Nutrition));
                                      var stackCount = bestFoodInInventory.stackCount;
                                      var bestFood = actor.inventory.innerContainer.Take(bestFoodInInventory, Mathf.Min(stackCountForNutrition, stackCount));

                                      actor.carryTracker.TryStartCarry(bestFood);
                                      actor.CurJob.SetTarget(TargetIndex.B, bestFood);

                                      var nutrition = bestFood.stackCount * bestFood.GetStatValue(StatDefOf.Nutrition);
                                      __instance.ticksLeftThisToil = Mathf.CeilToInt(270f * (nutrition / JobDriver_InteractAnimal.RequiredNutritionPerFeed(target)));

                                      if (stackCountForNutrition <= stackCount) { Traverse.Create(__instance).Field<float>("feedNutritionLeft").Value = 0f; }
                                      else
                                      {
                                          feedNutritionLeft.Value -= nutrition;

                                          if (feedNutritionLeft.Value >= 0.001f) { return; }
                                          feedNutritionLeft.Value = 0f;
                                      }
                                  }
                              };

            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            __result = toil;

            return false;
        }
    }
}
