using System;
using Harmony;
using PawnRules.Data;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(FoodUtility), "BestFoodSourceOnMap")]
    internal static class RimWorld_FoodUtility_BestFoodSourceOnMap
    {
        private static bool Prefix(ref Thing __result, Pawn getter, Pawn eater, bool desperate, out ThingDef foodDef, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false)
        {
            foodDef = null;

            if (Registry.ExemptedTrainer != null)
            {
                Registry.ExemptedTrainer = null;
                return true;
            }
            if (!Registry.IsActive) { return true; }

            var restriction = Registry.GetRules(eater)?.GetRestriction(RestrictionType.Food);
            if (eater.InMentalState || (restriction == null) || restriction.IsVoid) { return true; }

            var filtered = Access.Field_RimWorld_FoodUtility_Filtered_Get();

            var getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            if (!getterCanManipulate && (getter != eater))
            {
                Log.Error(getter + " tried to find food to bring to " + eater + " but " + getter + " is incapable of Manipulation.");
                __result = null;
                return false;
            }
            var minPref = !eater.NonHumanlikeOrWildMan() ? (!desperate ? (eater.needs.food.CurCategory < HungerCategory.UrgentlyHungry ? FoodPreferability.MealAwful : FoodPreferability.RawBad) : FoodPreferability.DesperateOnly) : FoodPreferability.NeverForNutrition;
            var foodValidator = (Predicate<Thing>) (thing =>
                                                    {
                                                        if (thing is Building_NutrientPasteDispenser nutrientPasteDispenser)
                                                        {
                                                            if (!allowDispenserFull || !getterCanManipulate || (ThingDefOf.MealNutrientPaste.ingestible.preferability < minPref) || (ThingDefOf.MealNutrientPaste.ingestible.preferability > maxPref) || !eater.RaceProps.CanEverEat(ThingDefOf.MealNutrientPaste) || ((thing.Faction != getter.Faction) && (thing.Faction != getter.HostFaction)) || (!allowForbidden && thing.IsForbidden(getter)) || !nutrientPasteDispenser.powerComp.PowerOn || (!allowDispenserEmpty && !nutrientPasteDispenser.HasEnoughFeedstockInHoppers()) || !thing.InteractionCell.Standable(thing.Map) || !Access.Method_RimWorld_FoodUtility_IsFoodSourceOnMapSociallyProper_Call(thing, getter, eater, allowSociallyImproper) || getter.IsWildMan() || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(thing.InteractionCell, thing.Map), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some))) { return false; }
                                                        }
                                                        else if ((thing.def.ingestible.preferability < minPref) || (thing.def.ingestible.preferability > maxPref) || !eater.RaceProps.WillAutomaticallyEat(thing) || !thing.def.IsNutritionGivingIngestible || !thing.IngestibleNow || (!allowCorpse && thing is Corpse) || (!allowDrug && thing.def.IsDrug) || (!allowForbidden && thing.IsForbidden(getter)) || (!desperate && thing.IsNotFresh()) || thing.IsDessicated() || !Access.Method_RimWorld_FoodUtility_IsFoodSourceOnMapSociallyProper_Call(thing, getter, eater, allowSociallyImproper) || (!getter.AnimalAwareOf(thing) && !forceScanWholeMap) || !getter.CanReserve(thing)) { return false; }

                                                        // Pawn Rules - Food check below
                                                        return restriction.AllowsFood(thing.def, eater);
                                                    });

            var request = ((eater.RaceProps.foodType & (FoodTypeFlags.Plant | FoodTypeFlags.Tree)) == FoodTypeFlags.None) || !allowPlant ? ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree) : ThingRequest.ForGroup(ThingRequestGroup.FoodSource);
            Thing bestThing;
            if (getter.RaceProps.Humanlike)
            {
                bestThing = Access.Method_RimWorld_FoodUtility_SpawnedFoodSearchInnerScan_Call(eater, getter.Position, getter.Map.listerThings.ThingsMatching(request), PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, foodValidator);

                if (allowHarvest && getterCanManipulate)
                {
                    var searchRegionsMax = !forceScanWholeMap || (bestThing != null) ? 30 : -1;
                    var firstBestThing = bestThing;

                    bool Validator(Thing thing)
                    {
                        var plant = (Plant) thing;
                        if (!plant.HarvestableNow) { return false; }
                        var harvestedThingDef = plant.def.plant.harvestedThingDef;

                        // Pawn Rules - Food check below
                        return harvestedThingDef.IsNutritionGivingIngestible && eater.RaceProps.CanEverEat(harvestedThingDef) && getter.CanReserve(plant) && (allowForbidden || !plant.IsForbidden(getter)) && ((firstBestThing == null) || (FoodUtility.GetFinalIngestibleDef(firstBestThing).ingestible.preferability < harvestedThingDef.ingestible.preferability)) && restriction.AllowsFood(plant.def, eater);
                    }

                    var foodSource = GenClosest.ClosestThingReachable(getter.Position, getter.Map, ThingRequest.ForGroup(ThingRequestGroup.HarvestablePlant), PathEndMode.Touch, TraverseParms.For(getter), 9999f, Validator, null, 0, searchRegionsMax);

                    if (foodSource != null)
                    {
                        bestThing = foodSource;
                        foodDef = FoodUtility.GetFinalIngestibleDef(foodSource, true);
                    }
                }

                if ((foodDef == null) && (bestThing != null)) { foodDef = FoodUtility.GetFinalIngestibleDef(bestThing); }
            }
            else
            {
                var maxRegionsToScan = Access.Method_RimWorld_FoodUtility_GetMaxRegionsToScan_Call(getter, forceScanWholeMap);
                filtered.Clear();

                foreach (var thing in GenRadial.RadialDistinctThingsAround(getter.Position, getter.Map, 2f, true))
                {
                    if (thing is Pawn pawn && (pawn != getter) && pawn.RaceProps.Animal && (pawn.CurJob != null) && (pawn.CurJob.def == JobDefOf.Ingest) && pawn.CurJob.GetTarget(TargetIndex.A).HasThing) { filtered.Add(pawn.CurJob.GetTarget(TargetIndex.A).Thing); }
                }

                var ignoreEntirelyForbiddenRegions = !allowForbidden && ForbidUtility.CaresAboutForbidden(getter, true) && (getter.playerSettings?.EffectiveAreaRestrictionInPawnCurrentMap != null);
                var predicate = (Predicate<Thing>) (thing => foodValidator(thing) && !filtered.Contains(thing) && (thing is Building_NutrientPasteDispenser || (thing.def.ingestible.preferability > FoodPreferability.DesperateOnly)) && !thing.IsNotFresh());
                var traverseParams = TraverseParms.For(getter);
                var validator = predicate;

                bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, request, PathEndMode.ClosestTouch, traverseParams, 9999f, validator, null, 0, maxRegionsToScan, false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
                filtered.Clear();

                if (bestThing == null) { bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, request, PathEndMode.ClosestTouch, traverseParams, 9999f, foodValidator, null, 0, maxRegionsToScan, false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions); }
                if (bestThing != null) { foodDef = FoodUtility.GetFinalIngestibleDef(bestThing); }
            }

            __result = bestThing;
            return false;
        }
    }
}
