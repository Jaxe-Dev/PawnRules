using HarmonyLib;
using PawnRules.Data;
using Verse.Profile;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(MemoryUtility), "ClearAllMapsAndWorld")]
    internal static class Verse_Profile_MemoryUtility_ClearAllMapsAndWorld
    {
        private static void Prefix() => Registry.Clear();
    }
}
