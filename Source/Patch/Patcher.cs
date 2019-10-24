using System.Reflection;
using Harmony;
using PawnRules.Data;
using Verse;

namespace PawnRules.Patch
{
    [StaticConstructorOnStartup]
    internal static class Patcher
    {
        public static HarmonyInstance Harmony { get; } = HarmonyInstance.Create(Mod.Id);

        static Patcher() => Harmony.PatchAll(Assembly.GetExecutingAssembly());

        public static void ApplyLanguageOverrides() => OverrideLanguageKey("FoodRestriction", Lang.Get("PresetType.Rules"));

        public static void OverrideLanguageKey(string key, string value)
        {
            var original = LanguageDatabase.activeLanguage.keyedReplacements[key];
            LanguageDatabase.activeLanguage.keyedReplacements.Remove(key);

            original.value = value;
            LanguageDatabase.activeLanguage.keyedReplacements.Add(key, original);
        }
    }
}
