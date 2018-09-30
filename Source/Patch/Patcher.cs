using System.Reflection;
using Harmony;
using Verse;

namespace PawnRules.Patch
{
    [StaticConstructorOnStartup]
    internal static class Patcher
    {
        public static HarmonyInstance Harmony { get; } = HarmonyInstance.Create(Mod.Id);

        static Patcher() => Harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}
