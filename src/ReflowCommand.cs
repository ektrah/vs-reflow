using System;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace Reflow
{
    internal sealed class ReflowCommand : IOleCommandTarget
    {
        private readonly IWpfTextView _view;

        public ReflowCommand(IWpfTextView view)
        {
            _view = view;
        }

        public IOleCommandTarget Next
        {
            get;
            internal set;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == PackageGuids.guidReflowPackageCmdSet)
            {
                if (nCmdID != PackageIds.cmdidReflow)
                {
                    return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
                }
                Reflow();
                return VSConstants.S_OK;
            }

            if (Next != null)
            {
                return Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            return (int)Constants.OLECMDERR_E_UNKNOWNGROUP;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == PackageGuids.guidReflowPackageCmdSet)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    if (prgCmds[i].cmdID != PackageIds.cmdidReflow)
                    {
                        return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
                    }
                    prgCmds[i].cmdf |= (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                }
                return VSConstants.S_OK;
            }

            if (Next != null)
            {
                return Next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }

            return (int)Constants.OLECMDERR_E_UNKNOWNGROUP;
        }

        private void Reflow()
        {
            var preferredLineLength = ReflowPackage.Options?.PreferredLineLength ?? ReflowOptionPage.DefaultLineLength;

            using (var edit = _view.TextBuffer.CreateEdit())
            {
                var snapshot = edit.Snapshot;

                var start = _view.Selection.Start.Position.Position;
                var end = _view.Selection.End.Position.Position;

                if (start == end)
                {
                    var line = snapshot.GetLineFromPosition(start);
                    if (string.IsNullOrWhiteSpace(line.GetText()))
                    {
                        edit.Cancel();
                        return;
                    }
                    start = line.Extent.Start;
                    end = line.Extent.End;
                    var startLine = line.LineNumber;
                    var endLine = line.LineNumber;
                    while (startLine - 1 >= 0)
                    {
                        line = snapshot.GetLineFromLineNumber(startLine - 1);
                        if (string.IsNullOrWhiteSpace(line.GetText()))
                        {
                            break;
                        }
                        startLine--;
                        start = line.Extent.Start;
                    }
                    while (endLine + 1 < snapshot.LineCount)
                    {
                        line = snapshot.GetLineFromLineNumber(endLine + 1);
                        if (string.IsNullOrWhiteSpace(line.GetText()))
                        {
                            break;
                        }
                        endLine++;
                        end = line.Extent.End;
                    }
                }

                var text = snapshot.GetText(start, end - start);

                var firstLine = snapshot.GetLineFromPosition(0);
                var lineEnding = (firstLine.LineBreakLength != 0) ? firstLine.GetLineBreakText() : _view.Options.GetNewLineCharacter();

                var tabSize = _view.Options.GetTabSize();
                var indentWidth = 0;
                var startLineStart = snapshot.GetLineFromPosition(start).Start.Position;
                var preamble = new char[start - startLineStart];
                snapshot.CopyTo(startLineStart, preamble, 0, preamble.Length);
                for (int i = 0; i < preamble.Length; i++)
                {
                    if (!char.IsWhiteSpace(preamble[i]))
                    {
                        preamble[i] = ' ';
                    }
                    indentWidth += (preamble[i] == '\t') ? tabSize - (indentWidth % tabSize) : 1;
                }

                var indent = 0;
                while (indent < text.Length && char.IsWhiteSpace(text, indent))
                {
                    indentWidth += (text[indent] == '\t') ? tabSize - (indentWidth % tabSize) : 1;
                    indent++;
                }

                var textLength = text.Length;
                while (textLength - 1 >= indent && char.IsWhiteSpace(text, textLength - 1))
                {
                    textLength--;
                }

                var sb = new StringBuilder();
                var lineLength = 0;
                var pos = indent;
                if (pos < textLength)
                {
                    var length = 0;
                    while (pos + length < textLength && !char.IsWhiteSpace(text, pos + length))
                    {
                        length++;
                    }
                    sb.Append(text, 0, indent).Append(text, pos, length);
                    lineLength = indentWidth + length;
                    pos += length;
                }
                while (pos < textLength)
                {
                    while (pos < textLength && char.IsWhiteSpace(text, pos))
                    {
                        pos++;
                    }
                    var length = 0;
                    while (pos + length < textLength && !char.IsWhiteSpace(text, pos + length))
                    {
                        length++;
                    }
                    if (lineLength + 1 + length > preferredLineLength)
                    {
                        sb.Append(lineEnding).Append(preamble, 0, preamble.Length).Append(text, 0, indent).Append(text, pos, length);
                        lineLength = indentWidth + length;
                    }
                    else
                    {
                        sb.Append(' ').Append(text, pos, length);
                        lineLength += 1 + length;
                    }
                    pos += length;
                }
                sb.Append(text, textLength, text.Length - textLength);

                var newText = sb.ToString();
                if (newText == text)
                {
                    edit.Cancel();
                    return;
                }

                edit.Replace(start, end - start, newText);
                edit.Apply();
            }
        }
    }
}
