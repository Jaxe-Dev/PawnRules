using System;
using PawnRules.Data;
using PawnRules.Patch;
using UnityEngine;
using Verse;

namespace PawnRules.Interface
{
    internal class Dialog_SetName : WindowPlus
    {
        private readonly string _label;

        private readonly Action<string> _onCommit;
        private readonly Func<string, bool> _validator;
        private string _name;

        private Dialog_SetName(string title, string label, Action<string> onCommit, Func<string, bool> validator, string name = "") : base(title, new Vector2(400f, 170f))
        {
            doCloseButton = false;
            closeOnClickedOutside = false;
            closeOnAccept = true;

            _label = label;
            _onCommit = onCommit;
            _validator = validator;
            _name = name ?? "";
        }

        public static void Open(string title, string label, Action<string> onCommit, Func<string, bool> validator, string name = null) => Find.WindowStack.Add(new Dialog_SetName(title, label, onCommit, validator, name));

        public override void OnAcceptKeyPressed() { CommitName(); }

        private void CommitName()
        {
            if (!NameIsValid()) { return; }

            _onCommit(_name);
            Close();
        }

        private bool NameIsValid() => (_validator == null) || _validator(_name);

        protected override void DoContent(Rect rect)
        {
            var listing = new Listing_StandardPlus();
            listing.Begin(rect);
            listing.Label(_label);
            GUI.SetNextControlName("NameField");
            _name = listing.TextEntry(_name);
            UI.FocusControl("NameField", this);
            listing.Gap();
            listing.End();

            var grid = rect.AdjustedBy(0f, listing.CurHeight, 0f, -listing.CurHeight).GetHGrid(4f, -1f, -1f);

            listing.Begin(grid[1]);
            if (listing.ButtonText(Lang.Get("Button.OK"), null, NameIsValid())) { CommitName(); }
            listing.End();

            listing.Begin(grid[2]);
            if (listing.ButtonText(Lang.Get("Button.Cancel"))) { Close(); }
            listing.End();
        }
    }
}
