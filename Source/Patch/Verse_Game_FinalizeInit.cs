using Harmony;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
    internal static class Verse_Game_FinalizeInit
    {
        private static void Postfix() => Controller.LoadWorld();
    }
}
