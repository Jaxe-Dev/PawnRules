using PawnRules.Data;
using PawnRules.Interface;
using Verse;

namespace PawnRules.Integration
{
    internal static class RimHUD
    {
        public static bool HideGizmo { get; set; } = false;

        public static string GetRulesInfo(Pawn pawn)
        {
            if (!Registry.IsActive) { return null; }

            var rules = Registry.GetOrNewRules(pawn);
            if (rules == null) { return null; }

            return rules.IsPreset ? rules.IsVoid ? null : rules.Name : Lang.Get("Preset.Personalized");
        }

        public static void OpenRulesDialog(Pawn pawn)
        {
            if (!Registry.IsActive) { return; }

            Dialog_Rules.Open(pawn);
        }
    }
}
