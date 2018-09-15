using System.Collections.Generic;
using System.Linq;
using PawnRules.Data;
using PawnRules.Patch;
using UnityEngine;
using Verse;

namespace PawnRules.Interface
{
    internal class Dialog_Plans : WindowPlus
    {
        private readonly Listing_StandardPlus _listing = new Listing_StandardPlus();
        private IEnumerable<string> _plans;
        private string _selected;

        private Dialog_Plans() : base(Lang.Get("Dialog_Plans.Title").Bold(), new Vector2(500f, 600f))
        {
            doCloseButton = false;

            GetPlans();
            _selected = _plans.FirstOrDefault();
        }

        public static void Open() => Find.WindowStack.Add(new Dialog_Plans());

        private void GetPlans()
        {
            _plans = Persistent.GetPlans();
            if (_selected == null) { _selected = _plans.FirstOrDefault(); }
        }

        private void LoadPlan()
        {
            Close();
            var loaded = Persistent.Load(_selected);
            Dialog_Alert.Open(Lang.Get(loaded ? "Dialog_Plans.LoadSuccess" : "Dialog_Plans.LoadFail", _selected));
        }

        private void SavePlan(string name)
        {
            Persistent.Save(name);
            _selected = name;
            GetPlans();
        }

        private void DeletePlan()
        {
            Persistent.DeletePlan(_selected);
            _selected = null;
            GetPlans();
        }

        protected override void DoContent(Rect rect)
        {
            var vGrid = rect.GetVGrid(4f, -1f, 30f, 30f);

            var hasPlans = _plans.Any();
            if (hasPlans)
            {
                _listing.Begin(vGrid[0], true);
                foreach (var plan in _plans)
                {
                    if (_listing.RadioButton(plan, _selected == plan)) { _selected = plan; }
                }
            }
            else
            {
                _listing.Begin(vGrid[0]);
                _listing.Label(Lang.Get("Dialog_Plans.NoneFound"));
            }
            _listing.End();

            var tGrid = vGrid[1].GetHGrid(4f, -1f, -1f);
            var bGrid = vGrid[2].GetHGrid(4f, -1f, -1f);
            if (GuiPlus.ButtonText(tGrid[0], Lang.Get("Dialog_Plans.Import"), Lang.Get("Dialog_Plans.ImportDesc"), !_selected.NullOrEmpty())) { LoadPlan(); }
            if (GuiPlus.ButtonText(tGrid[1], Lang.Get("Dialog_Plans.Delete"), Lang.Get("Dialog_Plans.DeleteDesc"), !_selected.NullOrEmpty())) { Dialog_Alert.Open(Lang.Get("Dialog_Plans.ConfirmDelete", _selected), Dialog_Alert.Buttons.YesNo, DeletePlan); }
            if (GuiPlus.ButtonText(bGrid[0], Lang.Get("Dialog_Plans.Export"), Lang.Get("Dialog_Plans.ExportDesc"))) { Dialog_SetName.Open(Lang.Get("Dialog_SetName.PlanTitle"), Lang.Get("Dialog_SetName.PlanLabel"), SavePlan, Persistent.NameIsValid, Persistent.CreateDefaultName()); }
            if (GuiPlus.ButtonText(bGrid[1], "CloseButton".Translate())) { Close(); }
        }
    }
}
