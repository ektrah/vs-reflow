using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Reflow
{
    internal sealed class ReflowCommand : IOleCommandTarget
    {
        private readonly IVsTextView _view;

        public ReflowCommand(IVsTextView view)
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
            if (pguidCmdGroup == PackageGuids.guidReflowPackageCmdSet && nCmdID == PackageIds.ReflowId)
            {
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
                    if (prgCmds[i].cmdID == PackageIds.ReflowId)
                    {
                        prgCmds[i].cmdf |= (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                    }
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
            int anchorLine, anchorColumn, endLine, endColumn;
            ErrorHandler.ThrowOnFailure(_view.GetSelection(out anchorLine, out anchorColumn, out endLine, out endColumn));

            if (anchorLine > endLine || anchorLine == endLine && anchorColumn > endColumn)
            {
                int tempLine = anchorLine;
                int tempColumn = anchorColumn;
                anchorLine = endLine;
                anchorColumn = endColumn;
                endLine = tempLine;
                endColumn = tempColumn;
            }

            IVsTextLines textLines;
            ErrorHandler.ThrowOnFailure(_view.GetBuffer(out textLines));

            if (anchorLine == endLine && anchorColumn == endColumn)
            {
                anchorColumn = 0;
                ErrorHandler.ThrowOnFailure(textLines.GetLengthOfLine(endLine, out endColumn));
            }

            string text;
            ErrorHandler.ThrowOnFailure(textLines.GetLineText(anchorLine, anchorColumn, endLine, endColumn, out text));

            int indent = 0;
            while (indent < text.Length && char.IsWhiteSpace(text, indent))
            {
                indent++;
            }

            StringBuilder sb = new StringBuilder();
            int size = 0;
            int pos = 0;
            while (pos < text.Length)
            {
                while (pos < text.Length && char.IsWhiteSpace(text, pos))
                {
                    pos++;
                }
                int length = 0;
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
                else if (size + 1 + length > 80)
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
                return;
            }

            IntPtr ptr = Marshal.StringToCoTaskMemAuto(newText);
            try
            {
                ErrorHandler.ThrowOnFailure(textLines.ReplaceLines(anchorLine, anchorColumn, endLine, endColumn, ptr, newText.Length, null));
            }
            finally
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }
    }
}