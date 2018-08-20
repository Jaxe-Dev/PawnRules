using System.Reflection;
using Harmony;
using PawnRules.Data;
using Verse;

namespace PawnRules
{
    [StaticConstructorOnStartup]
    internal static class Controller
    {
        static Controller() => HarmonyInstance.Create(Mod.Id).PatchAll(Assembly.GetExecutingAssembly());

        public static void LoadWorld()
        {
            AddonManager.AcceptingAddons = false;
            Registry.Initialize();
        }
    }
}
