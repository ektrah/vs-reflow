using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

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

                    var endLine = line.LineNumber;
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

                var indent = 0;
                while (indent < text.Length && char.IsWhiteSpace(text, indent))
                {
                    indent++;
                }

                var sb = new StringBuilder();
                var size = 0;
                var pos = 0;
                while (pos < text.Length)
                {
                    while (pos < text.Length && char.IsWhiteSpace(text, pos))
                    {
                        pos++;
                    }
                    var length = 0;
                    while (pos + length < text.Length && !char.IsWhiteSpace(text, pos + length))
                    {
                        length++;
                    }
                    if (size == 0)
                    {
                        sb.Append(text, 0, indent).Append(text, pos, length);
                        size = indent + length;
                    }
                    else if (length == 0)
                    {
                        sb.AppendLine();
                        size = 0;
                    }
                    else if (size + 1 + length > preferredLineLength)
                    {
                        sb.AppendLine().Append(text, 0, indent).Append(text, pos, length);
                        size = indent + length;
                    }
                    else
                    {
                        sb.Append(' ').Append(text, pos, length);
                        size += 1 + length;
                    }
                    pos += length;
                }

                string newText = sb.ToString();
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
