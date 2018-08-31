using PawnRules.Patch;
using Verse;

namespace PawnRules.Data
{
    internal class Binding : IExposable
    {
        public Pawn Pawn;
        public PawnType Target;
        public Rules Rules;

        private Rules _individual;
        private Rules _preset;

        public Binding()
        { }

        public Binding(Pawn pawn, Rules rules) : this()
        {
            Pawn = pawn;
            Rules = rules;
        }

        public Binding(PawnType target, Rules rules) : this()
        {
            Target = target;
            Rules = rules;
        }

        public void ExposeData()
        {
            if ((Scribe.mode != LoadSaveMode.Saving) || (Target == null)) { Scribe_References.Look(ref Pawn, "pawn"); }
            if ((Scribe.mode != LoadSaveMode.Saving) || (Target != null)) { Target = PawnType.FromId(ScribePlus.LookValue(Target?.Id, "target")); }

            if (Scribe.mode != LoadSaveMode.Saving)
            {
                Scribe_References.Look(ref _preset, "preset");
                if (Target == null)
                {
                    Scribe_Deep.Look(ref _individual, "rules");
                    _individual?.SetPawn(Pawn);
                }
            }
            else if (Rules != null)
            {
                if (Rules.IsPreset) { Scribe_References.Look(ref Rules, "preset"); }
                else { Scribe_Deep.Look(ref Rules, "rules"); }
            }

            if (Scribe.mode != LoadSaveMode.PostLoadInit) { return; }

            if (_preset != null) { Rules = _preset; }
            else if (_individual != null) { Rules = _individual; }
            else if (Pawn != null) { Rules = Registry.GetVoidPreset<Rules>(Pawn.GetTargetType()).CloneRulesFor(Pawn); }
            else { throw new Mod.Exception("Unable to load rules for binding"); }
        }
    }
}
