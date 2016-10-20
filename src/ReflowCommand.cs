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
        }
    }
}
