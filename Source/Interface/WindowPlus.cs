using UnityEngine;
using Verse;

namespace PawnRules.Interface
{
    internal abstract class WindowPlus : Window
    {
        public override Vector2 InitialSize { get; }
        protected string Title { get; set; }

        protected WindowPlus(Vector2 size) : this(null, size) { }

        protected WindowPlus(string title = null, Vector2 size = default)
        {
            draggable = true;
            doCloseX = true;
            doCloseButton = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;
            closeOnAccept = false;

            InitialSize = size == default ? new Vector2(800f, 600f) : size;
            Title = title;
        }

        protected abstract void DoContent(Rect rect);

        public override void DoWindowContents(Rect rect)
        {
            var wordWrap = Text.WordWrap;
            Text.WordWrap = false;

            DoContent(DoTitle(rect));

            Text.WordWrap = wordWrap;
        }

        private Rect DoTitle(Rect rect)
        {
            if (Title.NullOrEmpty()) { return rect; }

            var header = new Listing_StandardPlus();

            header.Begin(rect);
            header.LabelMedium(Title);
            header.GapLine();
            header.End();

            var contentRect = rect;
            contentRect.y += header.CurHeight;
            contentRect.height -= header.CurHeight;

            if (doCloseButton) { contentRect.height -= 55f; }

            return contentRect;
        }
    }
}
