using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Reflow
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public sealed class ReflowTextViewCreationListener : IVsTextViewCreationListener
    {
        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ReflowCommand command = new ReflowCommand(textViewAdapter);
            IOleCommandTarget next;
            ErrorHandler.ThrowOnFailure(textViewAdapter.AddCommandFilter(command, out next));
            command.Next = next;
        }
    }
}
