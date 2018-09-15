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
                                  var thing1 = FoodUtility.BestFoodInInventory(actor, target, FoodPreferability.NeverForNutrition, FoodPreferability.RawTasty);
                                  if (thing1 == null) { actor.jobs.EndCurrentJob(JobCondition.Incompletable); }
                                  else
                                  {
                                      actor.mindState.lastInventoryRawFoodUseTick = Find.TickManager.TicksGame;

                                      var stackCountForNutrition = FoodUtility.StackCountForNutrition(feedNutritionLeft.Value, thing1.GetStatValue(StatDefOf.Nutrition));
                                      var stackCount = thing1.stackCount;
                                      var thing2 = actor.inventory.innerContainer.Take(thing1, Mathf.Min(stackCountForNutrition, stackCount));

                                      actor.carryTracker.TryStartCarry(thing2);
                                      actor.CurJob.SetTarget(TargetIndex.B, thing2);

                                      var nutrition = thing2.stackCount * thing2.GetStatValue(StatDefOf.Nutrition);
                                      __instance.ticksLeftThisToil = Mathf.CeilToInt((270f * (nutrition / JobDriver_InteractAnimal.RequiredNutritionPerFeed(target))));

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
