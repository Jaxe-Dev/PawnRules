using Verse;

namespace PawnRules
{
    internal class Mod : Verse.Mod
    {
        public const string Id = "PawnRules";
        public const string Name = "Pawn Rules";
        public const string Author = "Jaxe";
        public const string Version = "1.0.9";

        public static ModContentPack ContentPack { get; private set; }

        public Mod(ModContentPack contentPack) : base(contentPack) => ContentPack = contentPack;

        public static void Log(string message) => Verse.Log.Message($"[{Name}] {message}");

        internal class Exception : System.Exception
        {
            public Exception(string message) : base($"[{Name} : EXCEPTION] {message}")
            { }
        }
    }
}
