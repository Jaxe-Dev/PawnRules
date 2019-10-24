using Harmony;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(Root), "Start")]
    internal static class Verse_Root_Start
    {
        private static void Postfix() => Mod.OnStartup();
    }
}
