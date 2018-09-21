using PawnRules.Data;
using Verse;

namespace PawnRules.Integration
{
    internal static class RimHUD
    {
        public static bool HideGizmo { get; set; } = false;

        public static string GetRulesInfo(Pawn pawn)
        {
            if (!Registry.IsActive) { return null; }

            var rules = Registry.GetRules(pawn);
            if (rules == null) { return null; }

            var name = rules.IsPreset ? rules.Name : Lang.Get("Preset.Personalized");
            return name;
        }
    }
}
