using System;
using System.Linq;
using PawnRules.Data;
using PawnRules.Patch;
using UnityEngine;

namespace PawnRules.Interface
{
    internal class Listing_Preset<T> where T : Presetable
    {
        private readonly Listing_StandardPlus _presetListing = new Listing_StandardPlus();
        private readonly Listing_StandardPlus _listing = new Listing_StandardPlus();

        public IPresetableType Type { get; set; }

        public Action OnSelect { get; }
        public Action OnSave { get; }
        public Action OnRevert { get; }

        public bool EditMode { get; set; }

        private T _lastSelected;
        public T Selected { get; set; }
        public T[] FixedPresets { get; set; }
        public bool IsUnsaved => _lastSelected != null;

        public Listing_Preset(IPresetableType type, T selected, T[] fixedPresets, Action onSelect, Action onSave, Action onRevert)
        {
            Type = type;
            Selected = selected;
            FixedPresets = fixedPresets;
            OnSelect = onSelect;
            OnSave = onSave;
            OnRevert = onRevert;
        }

        private void ChangeEditMode(bool value) => EditMode = value;

        private void ChangeSelected(T selected)
        {
            if (Selected == selected) { return; }
            Selected = selected;
            OnSelect?.Invoke();
        }

        public void DoContent(Rect rect)
        {
            var selectedIsIgnored = Selected.IsIgnored();
            var presets = Registry.GetPresets<T>(Type);

            _listing.Begin(rect);
            foreach (var preset in FixedPresets)
            {
                var isSelected = (Selected == preset) || (Selected.Name == preset.Name);
                if (_listing.RadioButtonInverted((preset.IsPreset ? preset.Name : Lang.Get("Preset.Personalized")).Italic(), isSelected, null, !EditMode || isSelected)) { ChangeSelected(preset); }
            }
            if (presets.Any()) { _listing.GapLine(); }
            _listing.End();

            var presetGrid = rect.GetVGrid(4f, _listing.CurHeight, -1f, 62f);

            _presetListing.Begin(presetGrid[1], true);
            foreach (var preset in presets)
            {
                var isSelected = (Selected == preset) || (Selected.Name == preset.Name);
                if (_presetListing.RadioButtonInverted(preset.Name, isSelected, null, !EditMode || isSelected)) { ChangeSelected(preset); }
            }
            _presetListing.End();

            var buttonGrid = presetGrid[2].GetHGrid(4f, -1f, -1f);
            _listing.Begin(buttonGrid[0]);

            if (_listing.ButtonText(Lang.Get("Button.PresetNew"), Lang.Get("Button.PresetNewDesc"), !EditMode)) { Presetable.SetName<T>(Type, CreatePreset); }

            if (EditMode)
            {
                if (_listing.ButtonText(Lang.Get("Button.PresetSave"), Lang.Get("Button.PresetSaveDesc")))
                {
                    Commit();
                    ChangeEditMode(false);
                    OnSave?.Invoke();
                }
            }
            else if (Selected.IsPreset)
            {
                if (_listing.ButtonText(Lang.Get("Button.PresetEdit"), Lang.Get("Button.PresetEditDesc"), !Selected.IsVoid)) { ChangeEditMode(true); }
            }
            else
            {
                if (_listing.ButtonText(Lang.Get("Button.PresetSaveAs"), Lang.Get("Button.PresetSaveAsDesc"), !Selected.IsVoid && !selectedIsIgnored)) { OnSave?.Invoke(); }
            }

            _listing.End();
            _listing.Begin(buttonGrid[1]);

            if (_listing.ButtonText(Lang.Get("Button.PresetDelete"), Lang.Get("Button.PresetDeleteDesc"), !Selected.IsVoid && Selected.IsPreset && !EditMode)) { Dialog_Alert.Open(Lang.Get("Button.PresetDeleteConfirm", Selected.Name), Dialog_Alert.Buttons.YesNo, DeletePreset); }

            if (EditMode)
            {
                if (_listing.ButtonText(Lang.Get("Button.PresetRevert"), Lang.Get("Button.PresetRevertDesc"), !Selected.IsVoid))
                {
                    Revert();

                    ChangeEditMode(false);
                    OnRevert?.Invoke();
                }
            }
            else if (Selected.IsPreset)
            {
                if (_listing.ButtonText(Lang.Get("Button.PresetRename"), Lang.Get("Button.PresetRenameDesc"), !Selected.IsVoid && !EditMode)) { Presetable.SetName(Selected, ChangeSelected); }
            }
            else
            {
                if (_listing.ButtonText(Lang.Get("Button.PresetClear"), Lang.Get("Button.PresetClearDesc"), !Selected.IsVoid && !selectedIsIgnored)) { OnRevert?.Invoke(); }
            }

            _listing.End();
        }

        private void DeletePreset()
        {
            Registry.DeletePreset(Selected);
            ChangeSelected(Registry.GetVoidPreset<T>(Type));
        }

        private void CreatePreset(T preset)
        {
            _lastSelected = Selected;
            ChangeEditMode(true);
            ChangeSelected(preset);
        }

        public void Revert()
        {
            if (!Selected.IsPreset || (_lastSelected == null)) { return; }
            Registry.DeletePreset(Selected);
            ChangeSelected(_lastSelected);
            Commit();
        }

        public void Commit() => _lastSelected = null;
    }
}
