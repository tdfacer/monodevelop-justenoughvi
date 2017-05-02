using System;
using Mono.TextEditor;

namespace JustEnoughVi
{
    public static class PreviousCommandInfo
    {
        public static Command PreviousCommand { get; set; }
        public static int Count { get; set; }
        public static char Argument { get; set; }
        public static string Text { get; set; }
    }

    public abstract class Command
    {
        public bool TakeArgument { get; protected set; }
        public int MinCount { get; protected set; }
        protected Mode RequestedMode { private get; set; }

        protected TextEditorData Editor { get; private set; }
        protected int Count { get; private set; }
        protected char Argument { get; private set; }

        protected Command(TextEditorData editor)
        {
            TakeArgument = false;
            MinCount = 1;
            Editor = editor;
        }

        public Mode RunCommand(int count, char arg, Command currentCommand = null)
        {
            Count = Math.Max(MinCount, count);
            Argument = arg;
            RequestedMode = Mode.None;

            if (currentCommand != null)
            {
                PreviousCommandInfo.PreviousCommand = currentCommand;
                PreviousCommandInfo.Count = count;
                PreviousCommandInfo.Argument = arg;
            }

            Run();

            return RequestedMode;
        }

        protected abstract void Run();

        // FIXME: refactor somewhere else?
        protected bool Dispatch(object command)
        {
            return MonoDevelop.Ide.IdeApp.CommandService.DispatchCommand(command);
        }

        protected void UpdatePrevious(string text = null)
        {
            PreviousCommandInfo.PreviousCommand = this;
            PreviousCommandInfo.Count = Count;
            PreviousCommandInfo.Argument = Argument;
            PreviousCommandInfo.Text = text;
        }

    }
}