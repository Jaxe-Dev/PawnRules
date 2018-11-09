using System.Collections.Generic;
using System.Linq;
using PawnRules.Data;
using PawnRules.Patch;
using UnityEngine;
using Verse;

namespace PawnRules.Interface
{
    internal class Dialog_Restrictions : WindowPlus
    {
        private readonly RestrictionType _type;
        private readonly Rules _rules;
        private RestrictionTemplate _template;

        private Color _color;

        public override Vector2 InitialSize => new Vector2(800f, 600f);

        private readonly Listing_StandardPlus _headerList = new Listing_StandardPlus();
        private readonly Listing_StandardPlus _categoryList = new Listing_StandardPlus();
        private readonly Listing_StandardPlus _membersList = new Listing_StandardPlus();
        private readonly Listing_Preset<Restriction> _presetList;

        private Dialog_Restrictions(RestrictionType type, Rules rules)
        {
            _rules = rules;
            _type = type;

            _presetList = new Listing_Preset<Restriction>(_type, _rules.GetRestriction(_type), new[] { Registry.GetVoidPreset<Restriction>(_type) }, RefreshTemplate, SaveTemplate, null);
            RefreshTemplate();
        }

        public static void Open(RestrictionType type, Rules rules) => Find.WindowStack.Add(new Dialog_Restrictions(type, rules));

        private void RefreshTemplate()
        {
            _template = RestrictionTemplate.Build(_type, _presetList.Selected);
            _rules.SetRestriction(_type, _presetList.Selected);
        }

        private void SaveTemplate() => _presetList.Selected.Update(_template);

        private bool HasMadeChanges() => !_presetList.Selected.IsVoid && !_rules.GetRestriction(_presetList.Selected.Type).Matches(_template);

        public override void Close(bool doCloseSound = true)
        {
            if (_presetList.EditMode && (_presetList.IsUnsaved || HasMadeChanges()))
            {
                void OnAccept()
                {
                    _presetList.Selected.Update(_template);
                    RefreshTemplate();
                    _presetList.Commit();
                    base.Close(doCloseSound);
                }

                void OnCancel()
                {
                    if (_presetList.IsUnsaved) { _presetList.Revert(); }

                    base.Close(doCloseSound);
                }

                Dialog_Alert.Open(Lang.Get("Button.PresetSaveConfirm"), Dialog_Alert.Buttons.YesNo, OnAccept, OnCancel);
                return;
            }

            base.Close(doCloseSound);
        }

        protected override void DoContent(Rect rect)
        {
            Title = _presetList.EditMode || (_rules.Pawn == null) ? Lang.Get("Dialog_Restrictions.TitlePreset", _type.Label, _presetList.Selected.Name.Bold()) : Lang.Get("Dialog_Restrictions.TitlePawn", _type.Label, _rules.Pawn.Name.ToStringFull.Bold(), _rules.Type.Label);

            _color = GUI.color;

            var vGrid = rect.GetVGrid(4f, 42f, -1f);
            DoHeader(vGrid[1]);

            var hGrid = vGrid[2].GetHGrid(8f, 200f, -1f, -1f);
            _presetList.DoContent(hGrid[1]);
            DoCategories(hGrid[2], hGrid[3]);
        }

        private void DoHeader(Rect rect)
        {
            var grid = rect.GetHGrid(8f, 200f, -1f, -1f);
            _headerList.Begin(grid[1]);
            _headerList.Label(Lang.Get("Preset.Header").Italic().Bold());
            _headerList.GapLine();
            _headerList.End();

            _headerList.Begin(grid[2]);
            _headerList.Label(Lang.Get("Dialog_Restrictions.HeaderCategory").Italic().Bold());
            _headerList.GapLine();
            _headerList.End();

            _headerList.Begin(grid[3]);
            _headerList.Label(_type.Categorization.Italic().Bold());
            _headerList.GapLine();
            _headerList.End();
        }

        private void DoCategories(Rect categoriesRect, Rect membersRect)
        {
            var vGrid = categoriesRect.GetVGrid(4f, -1f, 30f);
            _categoryList.Begin(vGrid[1], true);

            var members = new Dictionary<string, RestrictionTemplate.Toggle[]>();

            foreach (var category in _template.Categories)
            {
                var state = category.GetListState();

                var showAll = _presetList.EditMode || (_rules.Pawn == null);
                if (_type == RestrictionType.Food) { members[category.Label] = category.Members.Where(member => member.Def is ThingDef def && (!def.IsDrug || Registry.AllowDrugsRestriction) && (showAll || _rules.Pawn.RaceProps.CanEverEat(def))).ToArray(); }
                else if (_type == RestrictionType.Bonding) { members[category.Label] = category.Members.ToArray(); }
                else { throw new Mod.Exception("Unsupported restriction type"); }

                if (members[category.Label].Length == 0) { continue; }

                if (_presetList.EditMode)
                {
                    _categoryList.CheckboxPartial(category.Label, ref state);
                    category.UpdateState(state);
                }
                else
                {
                    GUI.color = GuiPlus.ReadOnlyColor;
                    _categoryList.CheckboxPartial(category.Label, ref state);
                    GUI.color = _color;
                }
            }

            _membersList.Gap();
            _categoryList.End();

            if (!_presetList.EditMode)
            {
                DoMembers(membersRect, members);
                return;
            }

            var hGrid = vGrid[2].GetHGrid(4f, -1f, -1f);
            var categoryButtons = new Listing_Standard();
            categoryButtons.Begin(hGrid[1]);
            if (categoryButtons.ButtonText(Lang.Get("Button.RestrictionsAllowOn"))) { _template.ToggleAll(true); }
            categoryButtons.End();
            categoryButtons.Begin(hGrid[2]);
            if (categoryButtons.ButtonText(Lang.Get("Button.RestrictionsAllowOff"))) { _template.ToggleAll(false); }
            categoryButtons.End();

            DoMembers(membersRect, members);
        }

        private void DoMembers(Rect membersRect, Dictionary<string, RestrictionTemplate.Toggle[]> categories)
        {
            _membersList.Begin(membersRect, true);

            foreach (var category in categories.Where(category => category.Value.Length > 0))
            {
                _membersList.LabelTiny(category.Key.Bold());

                foreach (var member in category.Value)
                {
                    if (_presetList.EditMode) { _membersList.CheckboxLabeled(member.Def.LabelCap, ref member.Value, member.Def.description); }
                    else
                    {
                        GUI.color = GuiPlus.ReadOnlyColor;
                        _membersList.CheckboxLabeled(member.Def.LabelCap, ref member.Value, member.Def.description, false);
                        GUI.color = _color;
                    }
                }
            }

            _membersList.Gap();
            _membersList.End();
        }
    }
}
