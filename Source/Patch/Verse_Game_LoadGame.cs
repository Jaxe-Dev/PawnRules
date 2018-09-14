using Harmony;
using PawnRules.Data;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
    internal static class Verse_Game_LoadGame
    {
        private static void Prefix() => Registry.Reset();
    }
}
