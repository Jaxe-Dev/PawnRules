using HarmonyLib;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    internal static class RimWorld_FoodUtility_WillEat
    {
        [HarmonyPatch(typeof(FoodUtility), "WillEat", typeof(Pawn), typeof(Thing), typeof(Pawn), typeof(bool))]
        private static class ByThing
        {
            public static void Postfix(ref bool __result, Pawn p, Thing food, Pawn getter = null) => ByThingDef.Postfix(ref __result, p, food.def, getter);
        }

        [HarmonyPatch(typeof(FoodUtility), "WillEat", typeof(Pawn), typeof(ThingDef), typeof(Pawn), typeof(bool))]
        private static class ByThingDef
        {
            public static void Postfix(ref bool __result, Pawn p, ThingDef food, Pawn getter = null)
            {
                if (!Registry.IsActive) { return; }

                if ((getter != null) && (Registry.ExemptedTrainer == getter)) { return; }

                if (Registry.AllowTrainingFood && (getter?.CurJobDef != null) && ((getter.CurJobDef == JobDefOf.Tame) || (getter.CurJobDef == JobDefOf.Train))) { return; }

                if (Registry.AllowRestingFood && (getter?.CurJobDef != null) && (getter.CurJobDef == JobDefOf.FeedPatient)) { return; }

                if (!p.RaceProps.CanEverEat(food)) { return; }

                var restriction = p.GetRules()?.GetRestriction(RestrictionType.Food);
                if ((restriction == null) || restriction.IsVoid || (p.InMentalState && ((getter == null) || (getter != p)))) { return; }

                __result = restriction.AllowsFood(food, p);
            }
        }
    }
}
