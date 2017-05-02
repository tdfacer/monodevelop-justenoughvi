using System;
using Mono.TextEditor;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Editor.Extension;
using System.Linq;

namespace JustEnoughVi
{
    public class FirstColumnCommand : Command
    {
        public FirstColumnCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Motion.FirstColumn(Editor);
        }
    }

    public class SaveAllFiles : Command
    {
        public SaveAllFiles(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            IdeApp.Workbench.SaveAll();
        }
    }

    public class AppendCommand : Command
    {
        public AppendCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            CaretMoveActions.Right(Editor);
            RequestedMode = Mode.Insert;
        }
    }

    public class AppendEndCommand : Command
    {
        public AppendEndCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Motion.LineEnd(Editor);
            RequestedMode = Mode.Insert;
        }
    }

    public class WordBackCommand : Command
    {
        public WordBackCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            for (int i = 0; i < Count; i++)
                Editor.Caret.Offset = StringUtils.PreviousWordOffset(Editor.Text, Editor.Caret.Offset);
        }
    }

    public class ChangeLineCommand : Command
    {
        public ChangeLineCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Motion.LineStart(Editor);
            int start = Editor.Caret.Offset;
            Motion.LineEnd(Editor);
            Editor.SetSelection(start, Editor.Caret.Offset);
            ClipboardActions.Cut(Editor);
            RequestedMode = Mode.Insert;
        }
    }

    public class ChangeWordCommand : DeleteWordEndCommand
    {
        public ChangeWordCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            base.Run();
            RequestedMode = Mode.Insert;
        }
    }

    public class ChangeWordEndCommand : DeleteWordEndCommand
    {
        public ChangeWordEndCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            base.Run();
            RequestedMode = Mode.Insert;
        }
    }

    public class DeleteToCharCommand : Command
    {
        private readonly int findResultShift;
        public DeleteToCharCommand(TextEditorData editor, int findResultShift) : base(editor)
        {
            TakeArgument = true;
            this.findResultShift = findResultShift;
        }

        protected override void Run()
        {
            for (int i = 0; i < Count; i++)
            {
                int start = Editor.Caret.Offset;
                var end = StringUtils.FindNextInLine(Editor.Text, Editor.Caret.Offset, Argument);
                if (end <= 0)
                    return;

                end += findResultShift;

                Editor.SetSelection(start, end);
                ClipboardActions.Cut(Editor);
            }
        }
    }

    public class ChangeToCharCommand : DeleteToCharCommand
    {
        public ChangeToCharCommand(TextEditorData editor, int findResultShift) : base(editor, findResultShift)
        {
        }

        protected override void Run()
        {
            base.Run();
            RequestedMode = Mode.Insert;
        }
    }

    public class ChangeToEndCommand : Command
    {
        public ChangeToEndCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            int start = Editor.Caret.Offset;
            CaretMoveActions.LineEnd(Editor);
            Editor.SetSelection(start, Editor.Caret.Offset);
            ClipboardActions.Cut(Editor);
            RequestedMode = Mode.Insert;
        }
    }

    public class DeleteLineCommand : Command
    {
        public DeleteLineCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            // hack for last line, it doesn't actually cut the line though
            if (Editor.Caret.Offset == Editor.Text.Length)
            {
                var line = Editor.GetLine(Editor.Caret.Line);
                if (line.Offset == line.EndOffset)
                {
                    DeleteActions.Backspace(Editor);
                    return;
                }
            }

            Motion.SetSelectLines(Editor, Editor.Caret.Line, Editor.Caret.Line + Count + (Count > 0 ? -1 : 0));
            ClipboardActions.Cut(Editor);
            Motion.LineStart(Editor);
        }
    }

    public class DeleteWordCommand : Command
    {
        public DeleteWordCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            int offset = Editor.Caret.Offset;
            for (int i = 0; i < Count && offset < Editor.Length; i++)
                offset = StringUtils.NextWordOffset(Editor.Text, offset);

            Editor.SetSelection(Editor.Caret.Offset, offset);
            ClipboardActions.Cut(Editor);        
        }
    }

    public class DeleteWordEndCommand : Command
    {
        public DeleteWordEndCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            int offset = Editor.Caret.Offset;
            for (int i = 0; i < Count && offset < Editor.Length; i++)
                offset = StringUtils.WordEndOffset(Editor.Text, offset);

            offset = (offset < Editor.Length) ? offset + 1 : offset;
            Editor.SetSelection(Editor.Caret.Offset, offset);
            ClipboardActions.Cut(Editor);                    
        }           
    }

    public class DeleteLineEndCommand : Command
    {
        public DeleteLineEndCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            var line = Editor.GetLine(Editor.Caret.Line);
            Editor.SetSelection(Editor.Caret.Offset, line.EndOffset);
            ClipboardActions.Cut(Editor);
        }
    }

    public class FindCommand : Command
    {
        readonly int findResultShift;

        public FindCommand(TextEditorData editor, int findResultShift) : base(editor)
        {
            TakeArgument = true;
            this.findResultShift = findResultShift;
        }

        protected override void Run()
        {
            for (int i = 0; i < Count; i++)
            {
                var offset = StringUtils.FindNextInLine(Editor.Text, Editor.Caret.Offset, Argument);
                if (offset <= 0)
                    return;

                Editor.Caret.Offset = offset;
            }

            Editor.Caret.Offset += findResultShift;
        }
    }

    public class FindPreviousCommand : Command
    {
        readonly int findResultShift;
        public FindPreviousCommand(TextEditorData editor, int findResultShift) : base(editor)
        {
            TakeArgument = true;
            this.findResultShift = findResultShift;
        }

        protected override void Run()
        {
            for (int i = 0; i < Count; i++)
            {
                var offset = StringUtils.FindPreviousInLine(Editor.Text, Editor.Caret.Offset, Argument);
                if (offset <= 0)
                    return;

                Editor.Caret.Offset = offset;
            }

            Editor.Caret.Offset += findResultShift;
        }
    }

    public class GoToFirstLineCommand : Command
    {
        public GoToFirstLineCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Editor.Caret.Line = 1;
            Motion.LineStart(Editor);
        }
    }

    public class GoToLineCommand : Command
    {
        public GoToLineCommand(TextEditorData editor) : base(editor)
        {
            MinCount = 0;
        }

        protected override void Run()
        {
            if (Count == 0)
            {
                CaretMoveActions.ToDocumentEnd(Editor);
            }
            else
            {
                Editor.Caret.Line = Count;
            }

            Motion.LineStart(Editor);
        }
    }

    public class GoToDeclarationCommand : Command
    {
        public GoToDeclarationCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Dispatch("MonoDevelop.Refactoring.RefactoryCommands.GotoDeclaration");
        }
    }

    public class GoToNextDocumentCommand : Command
    {
        public GoToNextDocumentCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Dispatch(WindowCommands.NextDocument);
        }
    }

    public class GoToPreviousDocumentCommand : Command
    {
        public GoToPreviousDocumentCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Dispatch(WindowCommands.PrevDocument);
        }
    }

    public class InsertCommand : Command
    {
        public InsertCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            RequestedMode = Mode.Insert;
        }
    }

    public class ReplaceModeCommand : Command
    {
        public ReplaceModeCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            RequestedMode = Mode.Replace;
        }
    }

    public class InsertLineStartCommand : Command
    {
        public InsertLineStartCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Motion.LineStart(Editor);
            RequestedMode = Mode.Insert;
        }
    }

    public class JoinCommand : Command
    {
        public JoinCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            for (int i = 0; i < Count; i++)
            {
                CaretMoveActions.LineEnd(Editor);
                int selectStart = Editor.Caret.Offset;

                while (Char.IsWhiteSpace(Editor.Text[Editor.Caret.Offset]))
                    Editor.Caret.Offset++;

                Editor.SetSelection(selectStart, Editor.Caret.Offset);
                Editor.DeleteSelectedText();
                Editor.InsertAtCaret(" ");
                Editor.Caret.Offset--;
            }
        }
    }

    public class SearchForwardCommand : Command
    {
        public SearchForwardCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            var currentWord = TextObject.CurrentWord(Editor);
            Editor.SetSelection(currentWord.Start, currentWord.End);
            Dispatch(SearchCommands.UseSelectionForFind);
            Dispatch(SearchCommands.Find);
            Dispatch(SearchCommands.FindNext);
            //Switch focus from find window back to document
            MonoDevelop.Ide.IdeApp.Workbench.ActiveDocument.Window.SelectWindow();
        }
    }

    public class SearchNextCommand : Command
    {
        public SearchNextCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Dispatch(SearchCommands.FindNext);
        }
    }

    public class SearchPreviousCommand : Command
    {
        public SearchPreviousCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Dispatch(SearchCommands.FindPrevious);
        }
    }

    public class OpenBelowCommand : Command
    {
        public OpenBelowCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            MiscActions.InsertNewLineAtEnd(Editor);
            RequestedMode = Mode.Insert;
        }
    }

    public class OpenAboveCommand : Command
    {
        public OpenAboveCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            if (Editor.Caret.Line == DocumentLocation.MinLine)
            {
                Editor.Caret.Column = 1;
                MiscActions.InsertNewLine(Editor);
                CaretMoveActions.Up(Editor);
            }
            else
            {
                CaretMoveActions.Up(Editor);
                MiscActions.InsertNewLineAtEnd(Editor);
            }
            RequestedMode = Mode.Insert;
        }
    }

    public class PasteAppendCommand : Command
    {
        public PasteAppendCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            // can the clipboard content be pulled without Gtk?
            var clipboard = Gtk.Clipboard.Get(ClipboardActions.CopyOperation.CLIPBOARD_ATOM);

            if (!clipboard.WaitIsTextAvailable())
                return;

            string text = clipboard.WaitForText();

            if (text.IndexOfAny(new char[] { '\r', '\n' }) > 0)
            {
                int oldOffset = Editor.Caret.Offset;
                var line = Editor.GetLine(Editor.Caret.Line);
                CaretMoveActions.LineEnd(Editor);
                Editor.Caret.Offset += line.DelimiterLength;
                Editor.InsertAtCaret(text);
                Editor.Caret.Offset = oldOffset;
                Motion.Down(Editor);
                Motion.LineStart(Editor);
            }
            else
            {
                Editor.Caret.Offset++;
                Editor.InsertAtCaret(text);
                Editor.Caret.Offset--;
            }
        }
    }

    public class PasteInsertCommand : Command
    {
        public PasteInsertCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            // can the clipboard content be pulled without Gtk?
            var clipboard = Gtk.Clipboard.Get(ClipboardActions.CopyOperation.CLIPBOARD_ATOM);

            if (!clipboard.WaitIsTextAvailable())
                return;

            string text = clipboard.WaitForText();

            if (text.IndexOfAny(new char[] { '\r', '\n' }) > 0)
            {
                if (Editor.Caret.Line == 1)
                {
                    Editor.Caret.Offset = 0;
                    Editor.InsertAtCaret(text);
                    Editor.Caret.Offset = 0;
                    Motion.LineStart(Editor);
                }
                else
                {
                    Motion.Up(Editor);
                    Motion.LineEnd(Editor);
                    Editor.Caret.Offset++;
                    int oldOffset = Editor.Caret.Offset;
                    Editor.InsertAtCaret(text);
                    Editor.Caret.Offset = oldOffset;
                    Motion.LineStart(Editor);
                }
            }
            else
            {
                Editor.InsertAtCaret(text);
                Editor.Caret.Offset--;
            }
        }
    }

    public class ReplaceCommand : Command
    {
        public ReplaceCommand(TextEditorData editor) : base(editor)
        {
            TakeArgument = true;
        }

        protected override void Run()
        {
            if (Char.IsControl(Argument))
                return;

            Editor.SetSelection(Editor.Caret.Offset, Editor.Caret.Offset + 1);
            DeleteActions.Delete(Editor);
            Editor.InsertAtCaret(Char.ToString(Argument));
            Editor.Caret.Offset--;
        }
    }

    public class UndoCommand : Command
    {
        public UndoCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            for (int i = 0; i < Count; i++)
                MiscActions.Undo(Editor);

            Editor.ClearSelection();
        }
    }

    public class VisualCommand : Command
    {
        public VisualCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            RequestedMode = Mode.Visual;
        }
    }

    public class VisualLineCommand : Command
    {
        public VisualLineCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            RequestedMode = Mode.VisualLine;
        }
    }

    public class WordCommand : Command
    {
        public WordCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            for (int i = 0; i < Count; i++)
                Editor.Caret.Offset = StringUtils.NextWordOffset(Editor.Text, Editor.Caret.Offset);
        }
    }

    public class WordEndCommand : Command
    {
        public WordEndCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            for (int i = 0; i < Count; i++)
                Editor.Caret.Offset = StringUtils.WordEndOffset(Editor.Text, Editor.Caret.Offset);
        }
    }

    public class DeleteCharacterCommand : Command
    {
        public DeleteCharacterCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            var count = Math.Min(Math.Max(Count, 1), Editor.GetLine(Editor.Caret.Line).EndOffset - Editor.Caret.Offset);
            Editor.SetSelection(Editor.Caret.Offset, Editor.Caret.Offset + count);
            ClipboardActions.Cut(Editor);
        }
    }

    public class ChangeCharacterCommand : Command
    {
        public ChangeCharacterCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            var count = Math.Min(Math.Max(Count, 1), Editor.GetLine(Editor.Caret.Line).EndOffset - Editor.Caret.Offset);
            Editor.SetSelection(Editor.Caret.Offset, Editor.Caret.Offset + count);
            ClipboardActions.Cut(Editor);

            RequestedMode = Mode.Insert;
        }
    }

    public class YankLineCommand : Command
    {
        public YankLineCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Motion.SetSelectLines(Editor, Editor.Caret.Line, Editor.Caret.Line + Count - 1);
            ClipboardActions.Copy(Editor);
            Editor.ClearSelection();
        }
    }

    public class RecenterCommand : Command
    {
        public RecenterCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Dispatch(TextEditorCommands.RecenterEditor);
        }
    }

    public class SearchCommand : Command
    {
        public SearchCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Dispatch(SearchCommands.Find);
        }
    }
    public class MatchingBraceCommand : Command
    {
        public MatchingBraceCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            MiscActions.GotoMatchingBracket(Editor);
        }
    }

    public class IndentCommand : Command
    {
        public IndentCommand(TextEditorData editor) : base(editor)
        {
            TakeArgument = true;
        }

        protected override void Run()
        {
            Motion.SetSelectLines(Editor, Editor.Caret.Line, Editor.Caret.Line + Count - 1);
            MiscActions.IndentSelection(Editor);
            Editor.ClearSelection();
        }
    }

    public class IndentOnceCommand : Command
    {
        public IndentOnceCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Motion.SetSelectLines(Editor, Editor.Caret.Line, Editor.Caret.Line);
            MiscActions.IndentSelection(Editor);
            Editor.ClearSelection();
        }
    }

    public class RemoveIndentCommand : Command
    {
        public RemoveIndentCommand(TextEditorData editor) : base(editor)
        {
            TakeArgument = true;
        }

        protected override void Run()
        {
            Motion.SetSelectLines(Editor, Editor.Caret.Line, Editor.Caret.Line + Count - 1);
            MiscActions.RemoveIndentSelection(Editor);
            Editor.ClearSelection();
        }
    }

    public class RemoveIndentOnceCommand : Command
    {
        public RemoveIndentOnceCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Motion.SetSelectLines(Editor, Editor.Caret.Line, Editor.Caret.Line);
            MiscActions.RemoveIndentSelection(Editor);
            Editor.ClearSelection();
        }
    }

    public class SwapCaseCommand : Command
    {
        public SwapCaseCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            var count = Math.Min(Math.Max(Count, 1), Editor.GetLine(Editor.Caret.Line).EndOffset - Editor.Caret.Offset);
            Editor.SetSelection(Editor.Caret.Offset, Editor.Caret.Offset + count);

            ClipboardActions.Copy(Editor);
            var stuff = ClipboardActions.GetClipboardContent();
            var swapped = new string(stuff.Select(c => char.IsLetter(c) ? char.IsUpper(c) ?
                    char.ToLower(c) : char.ToUpper(c) : c).ToArray());
            Editor.SetSelection(Editor.Caret.Offset, Editor.Caret.Offset + count);
            Editor.InsertAtCaret(swapped);
        }
    }

    public class NormalMode : ViMode
    {
        public NormalMode(TextEditorData editor) : base(editor)
        {
            // normal mode commands
            CommandMap.Add("0", new FirstColumnCommand(editor));
            CommandMap.Add("a", new AppendCommand(editor));
            CommandMap.Add("A", new AppendEndCommand(editor));
            CommandMap.Add("b", new WordBackCommand(editor));
            CommandMap.Add("cc", new ChangeLineCommand(editor));
            CommandMap.Add("S", new ChangeLineCommand(editor));
            CommandMap.Add("ci'", new ChangeCommand(editor, TextObject.InnerQuotedString, '\''));
            CommandMap.Add("ci\"", new ChangeCommand(editor, TextObject.InnerQuotedString, '\"'));
            CommandMap.Add("ci(", new ChangeInnerBlock(editor, '(', ')'));
            CommandMap.Add("ci)", new ChangeInnerBlock(editor, '(', ')'));
            CommandMap.Add("ci{", new ChangeInnerBlock(editor, '{', '}'));
            CommandMap.Add("ci}", new ChangeInnerBlock(editor, '{', '}'));
            CommandMap.Add("ci[", new ChangeInnerBlock(editor, '[', ']'));
            CommandMap.Add("ci]", new ChangeInnerBlock(editor, '[', ']'));
            CommandMap.Add("ci<", new ChangeInnerBlock(editor, '<', '>'));
            CommandMap.Add("ci>", new ChangeInnerBlock(editor, '<', '>'));
            CommandMap.Add("ca(", new ChangeCommand(editor, TextObject.Block, '(', ')'));
            CommandMap.Add("ca)", new ChangeCommand(editor, TextObject.Block, '(', ')'));
            CommandMap.Add("cab", new ChangeCommand(editor, TextObject.Block, '(', ')'));
            CommandMap.Add("ca{", new ChangeCommand(editor, TextObject.Block, '{', '}'));
            CommandMap.Add("ca}", new ChangeCommand(editor, TextObject.Block, '{', '}'));
            CommandMap.Add("caB", new ChangeCommand(editor, TextObject.Block, '{', '}'));
            CommandMap.Add("ca[", new ChangeCommand(editor, TextObject.Block, '[', ']'));
            CommandMap.Add("ca]", new ChangeCommand(editor, TextObject.Block, '[', ']'));
            CommandMap.Add("ca<", new ChangeCommand(editor, TextObject.Block, '<', '>'));
            CommandMap.Add("ca>", new ChangeCommand(editor, TextObject.Block, '<', '>'));
            CommandMap.Add("ca'", new ChangeCommand(editor, TextObject.QuotedString, '\''));
            CommandMap.Add("ca\"", new ChangeCommand(editor, TextObject.QuotedString, '\"'));
            CommandMap.Add("ca`", new ChangeCommand(editor, TextObject.QuotedString, '`'));
            CommandMap.Add("di[", new DeleteInnerBlock(editor, '[', ']'));
            CommandMap.Add("di]", new DeleteInnerBlock(editor, '[', ']'));
            CommandMap.Add("di'", new DeleteCommand(editor, TextObject.InnerQuotedString, '\''));
            CommandMap.Add("di\"", new DeleteCommand(editor, TextObject.InnerQuotedString, '"'));
            CommandMap.Add("di(", new DeleteInnerBlock(editor, '(', ')'));
            CommandMap.Add("di)", new DeleteInnerBlock(editor, '(', ')'));
            CommandMap.Add("di{", new DeleteInnerBlock(editor, '{', '}'));
            CommandMap.Add("di}", new DeleteInnerBlock(editor, '{', '}'));
            CommandMap.Add("di<", new DeleteInnerBlock(editor, '<', '>'));
            CommandMap.Add("di>", new DeleteInnerBlock(editor, '<', '>'));
            CommandMap.Add("cw", new ChangeWordCommand(editor));
            CommandMap.Add("ce", new ChangeWordEndCommand(editor));
            CommandMap.Add("dt", new DeleteToCharCommand(editor, 0));
            CommandMap.Add("df", new DeleteToCharCommand(editor, 1));
            CommandMap.Add("ct", new ChangeToCharCommand(editor, 0));
            CommandMap.Add("cf", new ChangeToCharCommand(editor, 1));
            CommandMap.Add("c$", new ChangeToEndCommand(editor));
            CommandMap.Add("C", new ChangeToEndCommand(editor));
            CommandMap.Add("dd", new DeleteLineCommand(editor));
            CommandMap.Add("dw", new DeleteWordCommand(editor));
            CommandMap.Add("de", new DeleteWordEndCommand(editor));
            CommandMap.Add("d$", new DeleteLineEndCommand(editor));
            CommandMap.Add("D", new DeleteLineEndCommand(editor));
            CommandMap.Add("f", new FindCommand(editor, 0));
            CommandMap.Add("F", new FindPreviousCommand(editor, 0));
            CommandMap.Add("t", new FindCommand(editor, -1));
            CommandMap.Add("T", new FindPreviousCommand(editor, 1));
            CommandMap.Add("gg", new GoToFirstLineCommand(editor));
            CommandMap.Add("gd", new GoToDeclarationCommand(editor));
            CommandMap.Add("gt", new GoToNextDocumentCommand(editor));
            CommandMap.Add("gT", new GoToPreviousDocumentCommand(editor));
            CommandMap.Add("G", new GoToLineCommand(editor));
            CommandMap.Add("i", new InsertCommand(editor));
            CommandMap.Add("I", new InsertLineStartCommand(editor));
            CommandMap.Add("J", new JoinCommand(editor));
            CommandMap.Add("n", new SearchNextCommand(editor));
            CommandMap.Add("N", new SearchPreviousCommand(editor));
            CommandMap.Add("o", new OpenBelowCommand(editor));
            CommandMap.Add("O", new OpenAboveCommand(editor));
            CommandMap.Add("p", new PasteAppendCommand(editor));
            CommandMap.Add("P", new PasteInsertCommand(editor));
            CommandMap.Add("r", new ReplaceCommand(editor));
            CommandMap.Add("u", new UndoCommand(editor));
            CommandMap.Add("v", new VisualCommand(editor));
            CommandMap.Add("V", new VisualLineCommand(editor));
            CommandMap.Add("w", new WordCommand(editor));
            CommandMap.Add("x", new DeleteCharacterCommand(editor));
            CommandMap.Add("yy", new YankLineCommand(editor));
            CommandMap.Add("Y", new YankLineCommand(editor));
            CommandMap.Add("zz", new RecenterCommand(editor));
            CommandMap.Add("/", new SearchCommand(editor));
            CommandMap.Add(">", new IndentCommand(editor));
            CommandMap.Add(">>", new IndentOnceCommand(editor));
            CommandMap.Add("<", new RemoveIndentCommand(editor));
            CommandMap.Add("<<", new RemoveIndentOnceCommand(editor));
            CommandMap.Add("%", new MatchingBraceCommand(editor));
            CommandMap.Add("e", new WordEndCommand(editor));
            CommandMap.Add("R", new ReplaceModeCommand(editor));
            CommandMap.Add("*", new SearchForwardCommand(editor));
            CommandMap.Add(":w", new SaveAllFiles(editor));
            CommandMap.Add("s", new ChangeCharacterCommand(editor));
            CommandMap.Add("~", new SwapCaseCommand(editor));


            // remaps
            SpecialKeyCommandMap.Add(SpecialKey.Delete, new DeleteCharacterCommand(editor));
        }

        #region implemented abstract members of ViMode

        protected override void Activate()
        {
            Editor.Caret.Mode = CaretMode.Block;
        }

        protected override void Deactivate()
        {
            Editor.Caret.Mode = CaretMode.Insert;
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            return base.KeyPress(descriptor);
        }

        #endregion
    }
}

