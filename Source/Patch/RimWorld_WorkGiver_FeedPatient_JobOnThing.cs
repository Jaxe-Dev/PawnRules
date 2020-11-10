using HarmonyLib;
using PawnRules.Data;
using RimWorld;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(WorkGiver_FeedPatient), "JobOnThing")]
    internal static class RimWorld_WorkGiver_FeedPatient_JobOnThing
    {
        private static void Prefix(Pawn pawn, Thing t)
        {
            if (!Registry.IsActive || !Registry.AllowRestingFood || !SickAnimalUtility.IsThingASickRestingAnimal(t)) { return; }

            Registry.ExemptedTrainer = pawn;
        }

        private static void Postfix()
        {
            if (!Registry.IsActive || (Registry.ExemptedTrainer == null)) { return; }

            Registry.ExemptedTrainer = null;
        }
    }

    [HarmonyPatch(typeof(WorkGiver_FeedPatient), "HasJobOnThing")]
    internal static class RimWorld_WorkGiver_FeedPatient_HasJobOnThing
    {
        private static void Prefix(Pawn pawn, Thing t)
        {
            if (!Registry.IsActive || !Registry.AllowRestingFood || !SickAnimalUtility.IsThingASickRestingAnimal(t)) { return; }

            Registry.ExemptedTrainer = pawn;
        }

        private static void Postfix()
        {
            if (!Registry.IsActive || (Registry.ExemptedTrainer == null)) { return; }

            Registry.ExemptedTrainer = null;
        }
    }

    public static class SickAnimalUtility
    {
        public static bool IsThingASickRestingAnimal(Thing t)
        {
            if (t is Pawn)
            {
                Pawn p = t as Pawn;
                return (p.training != null) && (HealthAIUtility.ShouldSeekMedicalRest(p) || !CanPawnWalk(p));
            }
            return false;
        }

        private static bool CanPawnWalk(Pawn p)
        {
            return p.health.capacities.CapableOf(PawnCapacityDefOf.Moving);
        }
    }

}
