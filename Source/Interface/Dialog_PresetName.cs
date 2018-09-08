using System;
using PawnRules.Data;
using PawnRules.Patch;
using UnityEngine;
using Verse;

namespace PawnRules.Interface
{
    internal class Dialog_PresetName<T> : WindowPlus where T : Presetable
    {
        private string _name;

        private bool _focused;
        private readonly T _preset;
        private readonly IPresetableType _type;
        private readonly Action<T> _onCommit;

        public Dialog_PresetName(string title) : base(title, new Vector2(400f, 170f))
        {
            doCloseButton = false;
            closeOnClickedOutside = false;
            closeOnAccept = true;
        }

        public Dialog_PresetName(IPresetableType type, Action<T> onCommit) : this(Lang.Get("Dialog_PresetName.TitleNew"))
        {
            _type = type;
            _onCommit = onCommit;
        }

        public Dialog_PresetName(T preset, Action<T> onCommit) : this(Lang.Get("Dialog_PresetName.Title", preset.Name))
        {
            _preset = preset;
            _name = _preset.Name;
            _onCommit = onCommit;
        }

        public override void OnAcceptKeyPressed()
        {
            if (!NameIsValid()) { return; }
            CommitName();
            base.OnAcceptKeyPressed();
        }

        private bool NameIsValid() => Presetable.NameIsValid<T>(_type ?? _preset.Type, _name);

        private void CommitName()
        {
            if (_type != null)
            {
                var preset = Registry.CreatePreset<T>(_type, _name);
                _onCommit(preset);
                return;
            }

            _onCommit(Registry.RenamePreset(_preset, _name));
        }

        public override void DoContent(Rect rect)
        {
            var listing = new Listing_StandardPlus();

            listing.Begin(rect);
            listing.Label(Lang.Get("Dialog_PresetName.Label"));
            GUI.SetNextControlName("RenameField");
            _name = listing.TextEntry(_name);
            var valid = NameIsValid();
            if (!_focused)
            {
                UI.FocusControl("RenameField", this);
                _focused = true;
            }
            listing.Gap();
            listing.End();

            var grid = rect.AdjustedBy(0f, listing.CurHeight, 0f, -listing.CurHeight).GetHGrid(4f, -1f, -1f);

            listing.Begin(grid[0]);
            if (listing.ButtonText(Lang.Get("Button.OK"), null, valid))
            {
                CommitName();
                Close();
            }
            listing.End();

            listing.Begin(grid[1]);
            if (listing.ButtonText(Lang.Get("Button.Cancel"))) { Close(); }
            listing.End();
        }
    }
}
