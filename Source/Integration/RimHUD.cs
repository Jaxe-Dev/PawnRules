using PawnRules.Data;
using PawnRules.Interface;
using PawnRules.Patch;
using Verse;

namespace PawnRules.Integration
{
    internal static class RimHUD
    {
        public static bool HideGizmo { get; set; } = false;

        public static string GetRules(Pawn pawn)
        {
            if (!Registry.IsActive) { return null; }

            var rules = Registry.GetOrNewRules(pawn);
            return rules?.GetDisplayName();
        }

        public static void OpenRules(Pawn pawn)
        {
            if (!Registry.IsActive) { return; }

            Dialog_Rules.Open(pawn);
        }
    }
}
