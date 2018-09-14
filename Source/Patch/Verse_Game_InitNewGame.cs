using Harmony;
using PawnRules.Data;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
    internal static class Verse_Game_InitNewGame
    {
        private static void Prefix() => Registry.Reset();
    }
}
