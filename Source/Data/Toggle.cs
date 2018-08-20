using Verse;

namespace PawnRules.Data
{
    internal class Toggle
    {
        public Def Def { get; }

        public bool Value;

        public Toggle(Def def, bool value)
        {
            Def = def;
            Value = value;
        }
    }
}
