using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using RimWorld;
using Verse;

namespace PawnRules.Data
{
    internal class Restriction : Presetable
    {
        public new RestrictionType Type { get => (RestrictionType) base.Type; private set => base.Type = value; }

        private List<string> _defs = new List<string>();

        public Restriction()
        { }

        public Restriction(RestrictionType type, string name) : base(name) => Type = type;

        public Restriction(XElement xml)
        {
            Name = xml.Element("Name")?.Value;
            if (Name.NullOrEmpty())
            {
                Mod.Warning("Skipping unnamed restriction preset");
                return;
            }

            Type = RestrictionType.FromId(xml.Attribute("Type")?.Value);
            if (Type == null)
            {
                Mod.Warning("Skipping invalid restriction type");
                return;
            }

            var defs = xml.Element("Defs")?.Elements();
            if (defs == null) { return; }

            foreach (var def in defs) { _defs.Add(def.Value); }
        }

        public bool Matches(RestrictionTemplate template) => _defs.SequenceEqual(from category in template.Categories from member in category.Members where !member.Value select member.Def.defName);

        public void Update(RestrictionTemplate template)
        {
            _defs.Clear();
            _defs.AddRange((from category in template.Categories from member in category.Members where !member.Value select member.Def.defName).ToList());
        }

        public bool Allows(Def def) => !_defs.Contains(def.defName);
        public bool AllowsFood(ThingDef def, Pawn pawn) => !_defs.Contains(def.defName) || (Registry.AllowEmergencyFood && (pawn.health?.hediffSet?.HasHediff(HediffDefOf.Malnutrition) ?? false));

        protected override void ExposePresetData()
        {
            Type = RestrictionType.FromId(ScribePlus.LookValue(Type?.Id, "type"));
            Scribe_Collections.Look(ref _defs, "restricted", LookMode.Value);
        }

        internal override bool IsIgnored() => _defs.Count == 0;

        protected override string GetPresetId() => $"Restriction_{Type.Id}_{Name}";

        public override XElement ToXml()
        {
            var xml = new XElement("Restriction", new XAttribute("Type", Type.Id));

            xml.Add(new XElement("Name", Name));
            xml.Add(new XElement("Defs", from def in _defs select new XElement("Def", def)));

            return xml;
        }
    }
}
