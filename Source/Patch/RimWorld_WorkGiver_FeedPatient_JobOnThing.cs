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
            if (!Registry.IsActive || !Registry.AllowRestingFood || !(t is Pawn) || ((t as Pawn).training is null)) { return; }

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
            if (!Registry.IsActive || !Registry.AllowRestingFood || !(t is Pawn) || ((t as Pawn).training is null)) { return; }

            Registry.ExemptedTrainer = pawn;
        }

        private static void Postfix()
        {
            if (!Registry.IsActive || (Registry.ExemptedTrainer == null)) { return; }

            Registry.ExemptedTrainer = null;
        }
    }

}
