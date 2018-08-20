using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnRules.Data
{
    internal class Restriction : Presetable
    {
        //public static readonly Restriction None = new Restriction(null, Lang.Get("Preset.None"));
        public new RestrictionType Type { get => (RestrictionType) base.Type; private set => base.Type = value; }

        private List<string> _defs = new List<string>();

        public Restriction()
        { }

        public Restriction(RestrictionType type, string name) : base(name) => Type = type;

        public Restriction GetRenamed(string name) => new Restriction(Type, name) { _defs = _defs };

        public bool Matches(RestrictionTemplate template) => _defs.SequenceEqual(from category in template.Categories from member in category.Members where !member.Value select member.Def.defName);

        public void Update(RestrictionTemplate template)
        {
            _defs.Clear();
            _defs.AddRange((from category in template.Categories from member in category.Members where !member.Value select member.Def.defName).ToList());
        }

        public bool Allows(Def def) => !_defs.Contains(def.defName);

        protected override void ExposePresetData()
        {
            Type = RestrictionType.FromId(ScribePlus.LookValue(Type?.Id, "type"));
            Scribe_Collections.Look(ref _defs, "restricted", LookMode.Value);
        }

        internal override bool IsIgnored() => _defs.Count == 0;

        protected override string GetPresetId() => $"Restriction_{Type.Id}_{Name}";
    }
}
