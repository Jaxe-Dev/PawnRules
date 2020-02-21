using HarmonyLib;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(Game), "FinalizeInit")]
    internal static class Verse_Game_FinalizeInit
    {
        private static void Postfix() => Mod.LoadWorld();
    }
}
