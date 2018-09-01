using System;
using Harmony;
using PawnRules.Data;
using RimWorld;
using UnityEngine.Profiling;
using Verse;
using Verse.AI;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap))]
    internal static class RimWorld_FoodUtility_BestFoodSourceOnMap
    {
        private static bool Prefix(ref Thing __result, Pawn getter, Pawn eater, bool desperate, out ThingDef foodDef, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false)
        {
            foodDef = null;

            if (!Registry.IsActive) { return true; }

            var rules = Registry.GetRules(eater);
            if (eater.InMentalState || (rules == null) || rules.GetRestriction(RestrictionType.Food).IsVoid) { return true; }

            Profiler.BeginSample("BestFoodInWorldFor getter=" + getter.LabelCap + " eater=" + eater.LabelCap);
            var getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            if (!getterCanManipulate && (getter != eater))
            {
                Log.Error(getter + " tried to find food to bring to " + eater + " but " + getter + " is incapable of Manipulation.");
                Profiler.EndSample();

                __result = null;
                return false;
            }

            var minPref = !eater.NonHumanlikeOrWildMan() ? (!desperate ? (eater.needs.food.CurCategory < HungerCategory.UrgentlyHungry ? FoodPreferability.MealAwful : FoodPreferability.RawBad) : FoodPreferability.DesperateOnly) : FoodPreferability.NeverForNutrition;
            var foodValidator = (Predicate<Thing>) (thing =>
                                                    {
                                                        Profiler.BeginSample("foodValidator");
                                                        if (!rules.GetRestriction(RestrictionType.Food).Allows(thing.def))
                                                        {
                                                            Profiler.EndSample();
                                                            return false;
                                                        }
                                                        if (thing is Building_NutrientPasteDispenser nutrientPasteDispenser)
                                                        {
                                                            if (!allowDispenserFull || !getterCanManipulate || (ThingDefOf.MealNutrientPaste.ingestible.preferability < minPref) || (ThingDefOf.MealNutrientPaste.ingestible.preferability > maxPref) || !eater.RaceProps.CanEverEat(ThingDefOf.MealNutrientPaste) || ((thing.Faction != getter.Faction) && (thing.Faction != getter.HostFaction)) || (!allowForbidden && thing.IsForbidden(getter)) || !nutrientPasteDispenser.powerComp.PowerOn || (!allowDispenserEmpty && !nutrientPasteDispenser.HasEnoughFeedstockInHoppers()) || !thing.InteractionCell.Standable(thing.Map) || !PrivateAccess.RimWorld_FoodUtility_IsFoodSourceOnMapSociallyProper(thing, getter, eater, allowSociallyImproper) || getter.IsWildMan() || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(thing.InteractionCell, thing.Map), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some)))
                                                            {
                                                                Profiler.EndSample();
                                                                return false;
                                                            }
                                                        }
                                                        else if ((thing.def.ingestible.preferability < minPref) || (thing.def.ingestible.preferability > maxPref) || !eater.RaceProps.WillAutomaticallyEat(thing) || !thing.def.IsNutritionGivingIngestible || !thing.IngestibleNow || (!allowCorpse && thing is Corpse) || (!allowDrug && thing.def.IsDrug) || (!allowForbidden && thing.IsForbidden(getter)) || (!desperate && thing.IsNotFresh()) || thing.IsDessicated() || !PrivateAccess.RimWorld_FoodUtility_IsFoodSourceOnMapSociallyProper(thing, getter, eater, allowSociallyImproper) || (!getter.AnimalAwareOf(thing) && !forceScanWholeMap) || !getter.CanReserve((LocalTargetInfo) thing))
                                                        {
                                                            Profiler.EndSample();
                                                            return false;
                                                        }

                                                        Profiler.EndSample();
                                                        return true;
                                                    });

            var req = ((eater.RaceProps.foodType & (FoodTypeFlags.Plant | FoodTypeFlags.Tree)) == FoodTypeFlags.None) || !allowPlant ? ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree) : ThingRequest.ForGroup(ThingRequestGroup.FoodSource);
            Thing bestThing;

            if (getter.RaceProps.Humanlike)
            {
                bestThing = PrivateAccess.RimWorld_FoodUtility_SpawnedFoodSearchInnerScan(eater, getter.Position, getter.Map.listerThings.ThingsMatching(req), PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, foodValidator);

                if (allowHarvest && getterCanManipulate)
                {
                    var searchRegionsMax = !forceScanWholeMap || (bestThing != null) ? 30 : -1;

                    bool Validator(Thing thing)
                    {
                        if (!rules.GetRestriction(RestrictionType.Food).Allows(thing.def)) { return false; }

                        var plant = (Plant) thing;
                        if (!plant.HarvestableNow) { return false; }

                        var harvestedThingDef = plant.def.plant.harvestedThingDef;
                        return harvestedThingDef.IsNutritionGivingIngestible && eater.RaceProps.CanEverEat(harvestedThingDef) && getter.CanReserve((LocalTargetInfo) plant) && (allowForbidden || !plant.IsForbidden(getter)) && ((bestThing == null) || (FoodUtility.GetFinalIngestibleDef(bestThing).ingestible.preferability < harvestedThingDef.ingestible.preferability));
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
                var maxRegionsToScan = PrivateAccess.RimWorld_FoodUtility_GetMaxRegionsToScan(getter, forceScanWholeMap);
                PrivateAccess.RimWorld_FoodUtility_Filtered().Clear();

                foreach (var thing in GenRadial.RadialDistinctThingsAround(getter.Position, getter.Map, 2f, true))
                {
                    if (thing is Pawn pawn && (pawn != getter) && pawn.RaceProps.Animal && (pawn.CurJob != null) && (pawn.CurJob.def == JobDefOf.Ingest) && pawn.CurJob.GetTarget(TargetIndex.A).HasThing) { PrivateAccess.RimWorld_FoodUtility_Filtered().Add(pawn.CurJob.GetTarget(TargetIndex.A).Thing); }
                }

                var flag = !allowForbidden && ForbidUtility.CaresAboutForbidden(getter, true) && (getter.playerSettings?.EffectiveAreaRestrictionInPawnCurrentMap != null);
                var predicate = (Predicate<Thing>) (thing => foodValidator(thing) && !PrivateAccess.RimWorld_FoodUtility_Filtered().Contains(thing) && (thing is Building_NutrientPasteDispenser || (thing.def.ingestible.preferability > FoodPreferability.DesperateOnly)) && !thing.IsNotFresh());
                var position1 = getter.Position;
                var map1 = getter.Map;
                var thingReq1 = req;
                var traverseParams1 = TraverseParms.For(getter);
                var validator1 = predicate;
                var ignoreEntirelyForbiddenRegions1 = flag;

                bestThing = GenClosest.ClosestThingReachable(position1, map1, thingReq1, PathEndMode.ClosestTouch, traverseParams1, 9999f, validator1, null, 0, maxRegionsToScan, false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions1);

                PrivateAccess.RimWorld_FoodUtility_Filtered().Clear();

                if (bestThing == null)
                {
                    desperate = true;
                    var position2 = getter.Position;
                    var map2 = getter.Map;
                    var thingReq2 = req;
                    var traverseParams2 = TraverseParms.For(getter);
                    var validator2 = foodValidator;
                    var ignoreEntirelyForbiddenRegions2 = flag;
                    bestThing = GenClosest.ClosestThingReachable(position2, map2, thingReq2, PathEndMode.ClosestTouch, traverseParams2, 9999f, validator2, null, 0, maxRegionsToScan, false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions2);
                }

                if (bestThing != null) { foodDef = FoodUtility.GetFinalIngestibleDef(bestThing); }
            }

            Profiler.EndSample();
            __result = bestThing;
            return false;
        }
    }
}
