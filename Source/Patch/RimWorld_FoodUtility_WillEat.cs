using Harmony;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    internal static class RimWorld_FoodUtility_WillEat
    {
        [HarmonyPatch(typeof(FoodUtility), "WillEat", typeof(Pawn), typeof(Thing), typeof(Pawn))]
        public static class FromThing
        {
            private static void Postfix(ref bool __result, Pawn p, Thing food, Pawn getter = null) => FromThingDef.Postfix(ref __result, p, food.def, getter);
        }

        [HarmonyPatch(typeof(FoodUtility), "WillEat", typeof(Pawn), typeof(ThingDef), typeof(Pawn))]
        private static class FromThingDef
        {
            public static void Postfix(ref bool __result, Pawn p, ThingDef food, Pawn getter = null)
            {
                if (!Registry.IsActive) { return; }

                if (Registry.ExemptedTrainer == getter) { return; }

                if (Registry.AllowTrainingFood && (getter?.CurJobDef != null) && ((getter.CurJobDef == JobDefOf.Tame) || (getter.CurJobDef == JobDefOf.Train))) { return; }

                if (!p.RaceProps.CanEverEat(food))
                {
                    __result = false;
                    return;
                }

                var restriction = p.GetRules()?.GetRestriction(RestrictionType.Food);
                if (p.InMentalState || (restriction == null) || restriction.IsVoid) { return; }

                __result = restriction.AllowsFood(food, p);
            }
        }
    }
}
