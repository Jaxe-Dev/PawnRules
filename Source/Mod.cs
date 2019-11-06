using System.IO;
using PawnRules.Data;
using PawnRules.Integration;
using PawnRules.Interface;
using PawnRules.Patch;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnRules
{
    internal class Mod : Verse.Mod
    {
        public const string Id = "PawnRules";
        public const string Name = "Pawn Rules";
        public const string Version = "1.3.2";

        public static readonly DirectoryInfo ConfigDirectory = new DirectoryInfo(Path.Combine(GenFilePaths.ConfigFolderPath, Id));

        public static Mod Instance { get; private set; }
        public static bool FirstTimeUser { get; private set; }

        public Mod(ModContentPack contentPack) : base(contentPack)
        {
            Instance = this;

            FirstTimeUser = !ConfigDirectory.Exists;
            ConfigDirectory.Create();

            if (!FirstTimeUser) { HugsLib.RegisterUpdateFeature(); }

            Log("Initialized");
        }

        public static void Log(string message) => Verse.Log.Message(PrefixMessage(message));
        public static void Warning(string message) => Verse.Log.Warning(PrefixMessage(message));
        public static void Message(string message) => Messages.Message(message, MessageTypeDefOf.TaskCompletion, false);

        public static string PrefixMessage(string message) => $"[{Name} v{Version}] {message}";
        public override string SettingsCategory() => Name;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var rect = inRect.GetHGrid(1f, -1f, 400f, -1f)[2];

            var listing = new Listing_Standard();
            listing.Begin(rect);

            if (Registry.IsActive)
            {
                if (listing.ButtonText(Lang.Get("Button.RemoveMod"), Lang.Get("Button.RemoveModDesc"))) { Dialog_Alert.Open(Lang.Get("Button.RemoveModConfirm"), Dialog_Alert.Buttons.YesNo, Registry.DeactivateMod); }
            }
            else { listing.Label(Lang.Get("Settings.NoGame")); }

            listing.End();
        }

        public static void OnStartup() => Patcher.ApplyLanguageOverrides();

        public static void LoadWorld()
        {
            AddonManager.AcceptingAddons = false;
            Registry.Initialize();
        }

        internal class Exception : System.Exception
        {
            public Exception(string message) : base(PrefixMessage(message)) { }
        }
    }
}
